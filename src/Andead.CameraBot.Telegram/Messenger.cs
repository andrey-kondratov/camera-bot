using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Andead.CameraBot.Media;
using Andead.CameraBot.Messaging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MihaZupan;
using MoreLinq;
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
        private static readonly UpdateType[] AllowedUpdates = {UpdateType.Message};
        private readonly TelegramBotClient _client;
        private readonly ILogger<Messenger> _logger;
        private readonly IOptions<TelegramOptions> _options;
        private CancellationToken _cancellationToken;

        private bool _isReceiving;

        public Messenger(IOptions<TelegramOptions> options, ILogger<Messenger> logger)
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
            _logger = logger;
        }

        public void StartReceiving(CancellationToken cancellationToken)
        {
            if (_isReceiving)
            {
                throw new InvalidOperationException("Already receiving. Stop me first.");
            }

            _isReceiving = true;
            _cancellationToken = cancellationToken;

            _client.StartReceiving(AllowedUpdates, cancellationToken);
            _client.OnMessage += OnMessage;

            _logger.LogInformation("Started receiving updates");
        }

        public void StopReceiving(CancellationToken cancellationToken)
        {
            _client.OnMessage -= OnMessage;
            _client.StopReceiving();

            _isReceiving = false;
            _logger.LogInformation("Stopped receiving updates");
        }

        public async Task SendSnapshot(Snapshot snapshot, ISnapshotRequest request, IEnumerable<string> cameraNames,
            CancellationToken cancellationToken)
        {
            var photo = new InputOnlineFile(snapshot.Stream);
            string caption = GetCaptionMarkdown(snapshot);
            IReplyMarkup replyMarkup = GetReplyMarkup(cameraNames);

            Message message = await _client.SendPhotoAsync(((SnapshotRequest) request).ChatId, photo, caption,
                ParseMode.Markdown, replyMarkup: replyMarkup, cancellationToken: cancellationToken);
            _logger.LogInformation("Responded with message {@Message} to request {@SnapshotRequest}", message, request);
        }

        public Task<bool> Test(CancellationToken cancellationToken)
        {
            return _client.TestApiAsync(cancellationToken);
        }

        public async Task SendGreeting(ISnapshotRequest request, IEnumerable<string> cameraNames,
            CancellationToken cancellationToken)
        {
            IReplyMarkup replyMarkup = GetReplyMarkup(cameraNames);
            Message message = await _client.SendTextMessageAsync(((SnapshotRequest) request).ChatId,
                "Send the camera name, I'll send a photo from it. ",
                replyMarkup: replyMarkup, cancellationToken: cancellationToken);
            _logger.LogInformation("Responded with greeting message {@Message} to request {@SnapshotRequest}", message,
                request);
        }

        public event EventHandler<SnapshotRequestedEventArgs> SnapshotRequested;

        private void OnMessage(object sender, MessageEventArgs e)
        {
            if (!TryCreateSnapshotRequest(e.Message, out SnapshotRequest snapshotRequest))
            {
                return;
            }

            EventHandler<SnapshotRequestedEventArgs> handler = SnapshotRequested;
            handler?.Invoke(this, new SnapshotRequestedEventArgs(snapshotRequest, _cancellationToken));
        }

        private bool TryCreateSnapshotRequest(Message message, out SnapshotRequest snapshotRequest)
        {
            string username = message.Chat.Username;
            string[] allowedUsernames = _options.Value.AllowedUsernames;
            if (!allowedUsernames.Any() || allowedUsernames.Contains(username))
            {
                long chatId = message.Chat.Id;
                string text = message.Text;

                _logger.LogInformation("Snapshot request received, chat id: {ChatId}, message: {@Message}", chatId,
                    message);
                snapshotRequest = new SnapshotRequest {Text = text, ChatId = chatId};
                return true;
            }

            _logger.LogWarning(
                "Message {@Message} discarded after comparing with allowed usernames {@AllowedUsernames}.",
                message, allowedUsernames);
            snapshotRequest = null;
            return false;
        }

        private static IReplyMarkup GetReplyMarkup(IEnumerable<string> cameraNames)
        {
            IEnumerable<IEnumerable<KeyboardButton>> GetKeyboard()
            {
                foreach (IEnumerable<string> row in cameraNames.Batch(3))
                {
                    yield return row.Select(id => new KeyboardButton(id));
                }
            }

            IEnumerable<IEnumerable<KeyboardButton>> keyboard = GetKeyboard();
            var markup = new ReplyKeyboardMarkup(keyboard);

            return markup;
        }

        private string GetCaptionMarkdown(Snapshot snapshot)
        {
            var builder = new StringBuilder();

            DateTime taken = snapshot.TakenUtc.AddHours(_options.Value.HoursOffset);
            builder.AppendFormat($"{{0:{_options.Value.DateTimeFormat}}}", taken);
            builder.AppendFormat(": {0}", snapshot.CameraName);
            if (!string.IsNullOrEmpty(snapshot.CameraUrl))
            {
                builder.AppendFormat(". [Watch live]({0})", snapshot.CameraUrl);
            }

            return builder.ToString();
        }
    }
}