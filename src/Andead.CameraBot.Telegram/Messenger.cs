using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Andead.CameraBot.Media;
using Andead.CameraBot.Messaging;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MihaZupan;
using MoreLinq;
using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;

namespace Andead.CameraBot.Telegram
{
    public class Messenger : IMessenger
    {
        private const int ReplyKeyboardWidth = 3;
        private static readonly UpdateType[] AllowedUpdates = { UpdateType.Message, UpdateType.CallbackQuery };
        private readonly TelegramBotClient _client;
        private readonly ILogger<Messenger> _logger;
        private readonly IOptions<TelegramOptions> _options;
        private readonly ICameraRegistry _registry;
        private readonly Uri _webhookUri;
        private CancellationToken _cancellationToken;

        public Messenger(IOptions<TelegramOptions> options, ICameraRegistry registry, ILogger<Messenger> logger)
        {
            Socks5Options socks5Options = options.Value.Socks5;

            if (!string.IsNullOrEmpty(socks5Options.Hostname))
            {
                _client = new TelegramBotClient(options.Value.ApiToken,
                    new HttpToSocks5Proxy(options.Value.Socks5.Hostname, options.Value.Socks5.Port));
            }
            else
            {
                _client = new TelegramBotClient(options.Value.ApiToken);
            }

            _options = options;
            _registry = registry;
            _logger = logger;

            Uri.TryCreate(_options.Value.Webhook.Url, UriKind.Absolute, out _webhookUri);
        }

        public async Task Start(CancellationToken cancellationToken = default)
        {
            _cancellationToken = cancellationToken;

            if (_webhookUri != null)
            {
                await _client.SetWebhookAsync(_webhookUri.ToString(), allowedUpdates: AllowedUpdates,
                    cancellationToken: cancellationToken);

                _logger.LogInformation("Webhook setup complete");
            }
            else
            {
                await _client.DeleteWebhookAsync(cancellationToken);

                _client.StartReceiving(AllowedUpdates, cancellationToken);
                _client.OnMessage += OnMessage;
                _client.OnCallbackQuery += OnCallbackQuery;

                _logger.LogInformation("Started receiving updates");
            }
        }

        public Task Stop(CancellationToken cancellationToken = default)
        {
            if (_client.IsReceiving)
            {
                _client.OnCallbackQuery -= OnCallbackQuery;
                _client.OnMessage -= OnMessage;
                _client.StopReceiving();
                _logger.LogInformation("Stopped receiving updates");
            }

            return Task.CompletedTask;
        }

        public async Task Navigate(Node node, ISnapshotRequest request, string alert = null, CancellationToken cancellationToken = default)
        {
            CallbackQuery query = ((SnapshotRequest)request).Query;
            long chatId = query.Message.Chat.Id;
            int requestMessageId = query.Message.MessageId;

            if (!string.IsNullOrWhiteSpace(alert))
            {
                await _client.AnswerCallbackQueryAsync(query.Id, alert, showAlert: true, cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
            }

            // update navigation controls in the current message
            InlineKeyboardMarkup replyMarkup = GetReplyMarkup(node);
            await _client.EditMessageReplyMarkupAsync(chatId, requestMessageId, replyMarkup, cancellationToken)
                .ConfigureAwait(false);

            _logger.LogInformation("Updated markup in message {@Message} in response to request {@SnapshotRequest}", query.Message, request);
        }

        public async Task SendSnapshotAndNavigate(Snapshot snapshot, ISnapshotRequest request, Node nodeToNavigate,
            CancellationToken cancellationToken = default)
        {
            CallbackQuery query = ((SnapshotRequest)request).Query;
            long chatId = query.Message.Chat.Id;
            int requestMessageId = query.Message.MessageId;

            InlineKeyboardMarkup replyMarkup;
            Message message;
            if (!snapshot.Success)
            {
                // notify user
                await _client.AnswerCallbackQueryAsync(query.Id, snapshot.Message, showAlert: true,
                    cancellationToken: cancellationToken).ConfigureAwait(false);

                // update the current message navigation controls
                replyMarkup = GetReplyMarkup(nodeToNavigate);
                await _client.EditMessageReplyMarkupAsync(chatId, requestMessageId, replyMarkup: replyMarkup,
                    cancellationToken: cancellationToken).ConfigureAwait(false);

                _logger.LogInformation("Updated markup in message {@Message} in response to request {@SnapshotRequest}",
                    query.Message, request);
                return;
            }

            // remove navigation controls from the current message
            replyMarkup = RemoveNavigationRows(query.Message.ReplyMarkup);
            await _client.EditMessageReplyMarkupAsync(chatId, requestMessageId, replyMarkup, cancellationToken)
                .ConfigureAwait(false);

            // post the snapshot and navigation controls in a new message
            var photo = new InputOnlineFile(snapshot.Stream);
            string caption = GetCaptionMarkdown(snapshot);
            replyMarkup = GetReplyMarkup(snapshot);

            message = await _client.SendPhotoAsync(chatId, photo, caption, ParseMode.Markdown, replyMarkup: replyMarkup,
                cancellationToken: cancellationToken).ConfigureAwait(false);

            _logger.LogInformation("Responded with message {@Message} to request {@SnapshotRequest}", message, request);
        }

        public Task<bool> Test(CancellationToken cancellationToken = default)
        {
            return _client.TestApiAsync(cancellationToken);
        }

        public event EventHandler<SnapshotRequestedEventArgs> SnapshotRequested;

        public Task Handle(IncomingRequest request, CancellationToken cancellationToken = default)
        {
            HttpRequest httpRequest = request.HttpRequest;
            if (httpRequest.Method == HttpMethods.Post &&
                _webhookUri.Host == httpRequest.Host.Value &&
                _webhookUri.AbsolutePath == httpRequest.Path.Value)
            {
                return TryHandleInternal(request, cancellationToken);
            }

            _logger.LogDebug("Invalid request headers: {Method}, {Host}, {Path}", httpRequest.Method,
                httpRequest.IsHttps, httpRequest.Host.Value, httpRequest.Path.Value);
            return Task.CompletedTask;
        }

        private async Task TryHandleInternal(IncomingRequest request, CancellationToken cancellationToken = default)
        {
            using var reader = new StreamReader(request.HttpRequest.Body);
            string payload = await reader.ReadToEndAsync();

            cancellationToken.ThrowIfCancellationRequested();

            Update update;
            try
            {
                update = JsonConvert.DeserializeObject<Update>(payload);
            }
            catch (JsonException exception)
            {
                _logger.LogError(exception, "Failed to deserialize payload");
                return;
            }

            request.Handled = true;
            SendGreeting(update.Message, _cancellationToken).GetAwaiter().GetResult();
        }

        private void OnMessage(object sender, MessageEventArgs e)
        {
            SendGreeting(e.Message, _cancellationToken).GetAwaiter().GetResult();
        }

        private async Task SendGreeting(Message message, CancellationToken cancellationToken)
        {
            long chatId = message.Chat.Id;

            string text = "Send the camera name, I'll send a photo from it.";

            Node root = await _registry.GetRootNode(cancellationToken: cancellationToken).ConfigureAwait(false);
            IReplyMarkup replyMarkup = GetReplyMarkup(root);

            Message response = await _client.SendTextMessageAsync(chatId, text, replyMarkup: replyMarkup,
                cancellationToken: cancellationToken).ConfigureAwait(false);

            _logger.LogInformation("Responded with greeting message {@Message} to chat id {ChatId}", response, chatId);
        }

        private void OnCallbackQuery(object sender, CallbackQueryEventArgs e)
        {
            CallbackQuery query = e.CallbackQuery;

            string username = query.Message.Chat.Username;
            string[] allowedUsernames = _options.Value.AllowedUsernames;
            if (allowedUsernames.Any() && !allowedUsernames.Contains(username))
            {
                _logger.LogWarning("Callback query {@Query} discarded after comparing with allowed usernames {@AllowedUsernames}.",
                    query, allowedUsernames);
                return;
            }

            string path = query.Data;

            var snapshotRequest = new SnapshotRequest { Path = path, Query = query };
            _logger.LogInformation("Snapshot request received: {@SnapshotRequest}", snapshotRequest);

            var args = new SnapshotRequestedEventArgs(snapshotRequest, _cancellationToken);
            EventHandler<SnapshotRequestedEventArgs> handler = SnapshotRequested;

            handler?.Invoke(this, args);
        }

        private static InlineKeyboardMarkup GetReplyMarkup(Node node)
        {
            IEnumerable<IEnumerable<InlineKeyboardButton>> keyboard = GetNavigationRows(node);
            var markup = new InlineKeyboardMarkup(keyboard);

            return markup;
        }

        private static InlineKeyboardMarkup GetReplyMarkup(Snapshot snapshot)
        {
            var row = new List<InlineKeyboardButton>();
            if (!string.IsNullOrEmpty(snapshot.Node.Url))
            {
                row.Add(InlineKeyboardButton.WithUrl("Watch live", snapshot.Node.Url));
            }

            if (!string.IsNullOrEmpty(snapshot.Node.Website))
            {
                row.Add(InlineKeyboardButton.WithUrl(new Uri(snapshot.Node.Website).Host, snapshot.Node.Website));
            }

            var rows = new List<IEnumerable<InlineKeyboardButton>>();
            rows.Add(row);

            foreach (IEnumerable<InlineKeyboardButton> navigationRow in GetNavigationRows(snapshot.Node.Parent))
            {
                rows.Add(navigationRow);
            }

            var replyMarkup = new InlineKeyboardMarkup(rows);
            return replyMarkup;
        }

        private static IEnumerable<IEnumerable<InlineKeyboardButton>> GetNavigationRows(Node node)
        {
            foreach (IEnumerable<Node> row in node.Children.Batch(ReplyKeyboardWidth))
            {
                yield return row.Select(node => InlineKeyboardButton.WithCallbackData(node.Name, node.Id));
            }

            if (node.Parent != null)
            {
                yield return new List<InlineKeyboardButton>
                {
                    InlineKeyboardButton.WithCallbackData("Back", node.Parent.Id)
                };
            }
        }

        private static InlineKeyboardMarkup RemoveNavigationRows(InlineKeyboardMarkup markup)
        {
            if (!string.IsNullOrWhiteSpace(markup.InlineKeyboard.FirstOrDefault()?.FirstOrDefault()?.CallbackData))
            {
                return new InlineKeyboardMarkup(Enumerable.Empty<IEnumerable<InlineKeyboardButton>>());
            }

            return new InlineKeyboardMarkup(markup.InlineKeyboard.Take(1));
        }

        private string GetCaptionMarkdown(Snapshot snapshot)
        {
            var builder = new StringBuilder();

            DateTime taken = snapshot.TakenUtc.AddHours(_options.Value.HoursOffset);
            builder.AppendFormat("*{0}*, ", snapshot.Node.Name);
            builder.AppendFormat($"{{0:{_options.Value.DateTimeFormat}}}", taken);

            return builder.ToString();
        }
    }
}