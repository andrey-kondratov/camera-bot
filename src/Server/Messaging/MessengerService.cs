using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MihaZupan;
using MoreLinq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;

namespace Andead.CameraBot.Server.Messaging
{
    class MessengerService : IMessengerService
    {
        private readonly IEnumerable<UpdateType> _allowedUpdates = new[] { UpdateType.Message };
        private readonly TelegramBotClient _client;
        private readonly IOptions<BotOptions> _options;
        private readonly ILogger<MessengerService> _logger;
        private int _lastUpdateId = 0;

        public MessengerService(IOptions<BotOptions> options, ILogger<MessengerService> logger)
        {
            Socks5Options socks5Options = options.Value.Telegram.Socks5;

            if (!string.IsNullOrEmpty(socks5Options.Hostname))
            {
                _client = new TelegramBotClient(options.Value.Telegram.ApiToken,
                    new HttpToSocks5Proxy(options.Value.Telegram.Socks5.Hostname, options.Value.Telegram.Socks5.Port));
            }
            else
            {
                _client = new TelegramBotClient(options.Value.Telegram.ApiToken);
            }

            _options = options;
            _logger = logger;
        }

        public async Task<SnapshotRequest> GetSnapshotRequest(CancellationToken cancellationToken)
        {
            int timeout = _options.Value.Telegram.Updates.Timeout;
            string[] allowedUsernames = _options.Value.Telegram.AllowedUsernames;

            while (!cancellationToken.IsCancellationRequested)
            {
                int offset = _lastUpdateId + 1;

                Update[] updates;
                try
                {
                    updates = await _client.GetUpdatesAsync(offset, 1, timeout, _allowedUpdates, cancellationToken);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
                catch (Exception exception)
                {
                    _logger.LogError(exception, "Error getting updates for offset {Offset}, timeout {Timeout}", offset, timeout);
                    break;
                }

                if (!updates.Any())
                {
                    _logger.LogDebug("No updates yet.");
                    break;
                }

                Update update = updates[0];
                Message message = update.Message;
                _lastUpdateId = update.Id;

                string username = message.Chat.Username;
                if (!allowedUsernames.Any() || allowedUsernames.Contains(username))
                {
                    long chatId = message.Chat.Id;
                    string text = message.Text;

                    _logger.LogInformation("Snapshot request received, chat id: {ChatId}, message: {@Message}", chatId, message);
                    return new SnapshotRequest { Text = text, ChatId = chatId };
                }

                _logger.LogWarning("Message {@Message} discarded after comparing with allowed usernames {@AllowedUsernames}.",
                    message, allowedUsernames);

                await Task.Delay(timeout, cancellationToken);
            }

            return null;
        }

        public async Task SendOops(long chatId, IEnumerable<string> cameraNames, CancellationToken cancellationToken)
        {
            try
            {
                IReplyMarkup replyMarkup = GetReplyMarkup(cameraNames);
                await _client.SendTextMessageAsync(chatId, "Something went wrong. Try again.", 
                    replyMarkup: replyMarkup, cancellationToken: cancellationToken);
            }
            catch (TaskCanceledException)
            {
                // ignored
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Error sending Oops message to chat {ChatId}", chatId);
            }
        }

        public async Task SendSnapshot(Snapshot snapshot, long chatId, IEnumerable<string> cameraNames, CancellationToken cancellationToken)
        {
            try
            {
                InputOnlineFile photo = GetInputOnlineFile(snapshot);
                string caption = GetCaptionMarkdown(snapshot);
                IReplyMarkup replyMarkup = GetReplyMarkup(cameraNames);

                await _client.SendPhotoAsync(chatId, photo, caption, ParseMode.Markdown, 
                    replyMarkup: replyMarkup, cancellationToken: cancellationToken);
                _logger.LogInformation("Snapshot sent to chat {ChatId}", chatId);
            }
            catch (TaskCanceledException)
            {
                // ignored
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Error sending snapshot to chat {ChatId}", chatId);
            }
        }

        public async Task<bool> Test(CancellationToken cancellationToken)
        {
            try
            {
                bool result = await _client.TestApiAsync(cancellationToken);

                if (!result)
                {
                    _logger.LogError("API test failed");
                }
                else
                {
                    _logger.LogInformation("API test successful");
                }

                return result;
            }
            catch (TaskCanceledException)
            {
                // ignored
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Error during API test");
            }

            return false;
        }

        private IReplyMarkup GetReplyMarkup(IEnumerable<string> cameraNames)
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

        private InputOnlineFile GetInputOnlineFile(Snapshot snapshot)
        {
            if (!string.IsNullOrEmpty(snapshot.Url))
            {
                return new InputOnlineFile(snapshot.Url);
            }

            return new InputOnlineFile(snapshot.Stream);
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
