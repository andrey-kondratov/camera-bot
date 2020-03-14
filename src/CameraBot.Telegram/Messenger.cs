using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using CameraBot.Media;
using CameraBot.Messaging;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MihaZupan;
using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace CameraBot.Telegram
{
    internal class Messenger : IMessenger
    {
        private static readonly UpdateType[] AllowedUpdates = { UpdateType.Message, UpdateType.CallbackQuery };
        private readonly ILogger<Messenger> _logger;
        private readonly TelegramOptions _options;
        private readonly ICameraRegistry _registry;
        private readonly TelegramBotClient _client;
        private CancellationToken _cancellationToken;
        private User _me;
        private Uri _webhookUri;
        private bool _started;

        public Messenger(IOptions<TelegramOptions> options, ICameraRegistry registry, ILogger<Messenger> logger)
        {
            _options = options.Value;
            _registry = registry;
            _logger = logger;

            // create the client
            string apiToken = _options.ApiToken;
            if (!string.IsNullOrEmpty(_options.Socks5?.Hostname))
            {
                IWebProxy webProxy = new HttpToSocks5Proxy(_options.Socks5.Hostname, _options.Socks5.Port);
                _client = new TelegramBotClient(apiToken, webProxy);
            }
            else
            {
                _client = new TelegramBotClient(apiToken);
            }
        }

        public async Task Start(CancellationToken cancellationToken = default)
        {
            // remember the "global" cancellation token so we can still pass it to methods
            // when no other is at hand
            _cancellationToken = cancellationToken;

            _me = await _client.GetMeAsync(cancellationToken);

            // attempt enabling webhook mode
            if (Uri.TryCreate(_options.Webhook.Url, UriKind.Absolute, out Uri uri))
            {
                _webhookUri = uri;
                await _client.SetWebhookAsync(_webhookUri.ToString(), allowedUpdates: AllowedUpdates,
                    cancellationToken: cancellationToken);

                _started = true;
                _logger.LogInformation($"Telegram started in webhook mode");
                return;
            }

            // remove any webhook URL from the server, start long-polling
            await _client.DeleteWebhookAsync(cancellationToken);

            _client.StartReceiving(AllowedUpdates, cancellationToken);
            _client.OnMessage += OnMessage;
            _client.OnCallbackQuery += OnCallbackQuery;

            _started = true;
            _logger.LogInformation("Telegram started in long-polling mode");
        }
        public Task Stop(CancellationToken cancellationToken = default)
        {
            if (!_started)
            {
                throw new InvalidOperationException("Messenger was not started.");
            }

            // check if long-polling is going on
            if (_client.IsReceiving)
            {
                _client.OnCallbackQuery -= OnCallbackQuery;
                _client.OnMessage -= OnMessage;
                _client.StopReceiving();

                _logger.LogInformation("Long-polling stopped");
            }

            // the webhook URL is not removed from the server here 
            // in case other bot replicas are running
            return Task.CompletedTask;
        }

        public async Task Navigate(Node node, ISnapshotRequest request, string alert = null,
            CancellationToken cancellationToken = default)
        {
            if (!_started)
            {
                throw new InvalidOperationException("Messenger was not started.");
            }

            await _client.Navigate(node, request, alert, cancellationToken);

            _logger.LogInformation("User {UserName} navigated to {NodeName}",
                ((SnapshotRequest)request).Query.From.Username, node.Name);
        }

        public async Task SendSnapshotAndNavigate(Snapshot snapshot, ISnapshotRequest request, Node nodeToNavigate,
            CancellationToken cancellationToken = default)
        {
            if (!_started)
            {
                throw new InvalidOperationException("Messenger was not started.");
            }

            await _client.ClearLastKeyboard(request, cancellationToken).ConfigureAwait(false);

            Message message = snapshot.Success
                ? await _client.SendSnapshotSuccess(snapshot, request, nodeToNavigate, _options, cancellationToken).ConfigureAwait(false)
                : await _client.SendSnapshotFailure(snapshot, request, nodeToNavigate, cancellationToken).ConfigureAwait(false);

            _logger.LogInformation("Sent snapshot message to {UserName}", message.Chat.Username);
        }

        public Task<bool> Test(CancellationToken cancellationToken = default)
        {
            return _client.TestApiAsync(cancellationToken);
        }

        public event EventHandler<SnapshotRequestedEventArgs> SnapshotRequested;

        public Task Handle(IncomingRequest request, CancellationToken cancellationToken = default)
        {
            if (!_started)
            {
                throw new InvalidOperationException("Messenger was not started.");
            }

            HttpRequest httpRequest = request.HttpRequest;
            if (httpRequest.Method != HttpMethods.Post || _webhookUri.Host != httpRequest.Host.Value ||
                _webhookUri.AbsolutePath != httpRequest.Path.Value)
            {
                _logger.LogWarning("Invalid request headers: {Method}, {Host}, {Path}", httpRequest.Method,
                    httpRequest.IsHttps, httpRequest.Host.Value, httpRequest.Path.Value);

                return Task.CompletedTask;
            }

            return HandleAsync();
            async Task HandleAsync()
            {
                using var reader = new StreamReader(request.HttpRequest.Body);
                string payload = await reader.ReadToEndAsync();

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

                switch (update.Type)
                {
                    case UpdateType.CallbackQuery:
                        OnCallbackQuery(update.CallbackQuery, cancellationToken);
                        return;
                    case UpdateType.Message:
                        await OnMessage(update.Message, cancellationToken);
                        return;
                    default:
                        _logger.LogWarning("Ignored update with type {UpdateType}", update.Type);
                        return;
                }
            }
        }

        private void OnMessage(object _, MessageEventArgs e)
        {
            OnMessage(e.Message, _cancellationToken).GetAwaiter().GetResult();
        }

        private Task OnMessage(Message message, CancellationToken cancellationToken)
        {
            if (!IsUsernameValid(message.Chat.Username))
            {
                _logger.LogWarning("Message from {UserName} ignored", message.Chat.Username);

                return Task.CompletedTask;
            }

            switch (message.Text)
            {
                case Constants.StartCommand:
                    return Greet(message, cancellationToken);
                case Constants.SnapshotCommand:
                    return PromptSnapshot(message, cancellationToken);
                case Constants.FeedbackCommand:
                    return PromptFeedback(message, cancellationToken);
                default:
                    return IsReplyToFeedbackPrompt(message)
                        ? OnFeedbackMessage(message, cancellationToken)
                        : OnBadRequest(message, cancellationToken);
            }
        }

        private async Task Greet(Message message, CancellationToken cancellationToken)
        {
            await _client.Greet(message, cancellationToken);

            _logger.LogInformation("Greeted user {UserName}", message.Chat.Username);
        }

        private async Task OnBadRequest(Message message, CancellationToken cancellationToken)
        {
            await _client.SendBadRequest(message, cancellationToken).ConfigureAwait(false);

            _logger.LogWarning("Bad request {Text} from {UserName}", message.Text, message.Chat.Username);
        }

        private async Task PromptSnapshot(Message message, CancellationToken cancellationToken)
        {
            Node root = await _registry.GetRootNode(cancellationToken).ConfigureAwait(false);
            await _client.PromptSnapshot(message, root, cancellationToken).ConfigureAwait(false);

            _logger.LogInformation("Sent snapshot prompt to {UserName}", message.Chat.Username);
        }

        private async Task PromptFeedback(Message message, CancellationToken cancellationToken)
        {
            await _client.PromptFeedback(message, cancellationToken).ConfigureAwait(false);

            _logger.LogInformation("Sent feedback prompt to {UserName}", message.Chat.Username);
        }

        private void OnCallbackQuery(object _, CallbackQueryEventArgs e)
        {
            OnCallbackQuery(e.CallbackQuery, _cancellationToken);
        }

        private void OnCallbackQuery(CallbackQuery query, CancellationToken cancellationToken)
        {
            if (!IsUsernameValid(query.Message.Chat.Username))
            {
                _logger.LogWarning("Callback query from {UserName} ignored", query.Message.Chat.Username);

                return;
            }

            var snapshotRequest = new SnapshotRequest { Id = query.Data, Query = query };

            _logger.LogInformation("Snapshot request {Id} received from {UserName}", snapshotRequest.Id, query.From.Username);

            var args = new SnapshotRequestedEventArgs(snapshotRequest, cancellationToken);
            SnapshotRequested?.Invoke(this, args);
        }

        private bool IsUsernameValid(string username)
        {
            return !_options.AllowedUsernames.Any() || _options.AllowedUsernames.Contains(username);
        }

        private bool IsReplyToFeedbackPrompt(Message message)
        {
            Message replyTo = message.ReplyToMessage;
            return replyTo != null && replyTo.From.Username == _me.Username &&
                replyTo.Text == MessageHelpers.FeedbackPromptMarkdown;
        }

        private async Task OnFeedbackMessage(Message message, CancellationToken cancellationToken)
        {
            // send a message to the author
            var chatId = _options.Feedback.ChatId;
            if (chatId.HasValue)
            {
                string text = MessageHelpers.GetFeedbackText(message, _options.Feedback.Header);
                await _client.SendTextMessageAsync(chatId.Value, text, ParseMode.Default,
                    disableWebPagePreview: true, cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

                await _client.SendTextMessageAsync(message.Chat.Id, MessageHelpers.FeedbackResponseMarkdown,
                    ParseMode.Markdown, cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

                return;
            }

            _logger.LogWarning("Feedback from {UserName} discarded: no feedback chat id configured.",
                message.From.Username);
        }
    }
}