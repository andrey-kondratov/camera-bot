using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MihaZupan;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;

namespace Andead.CameraBot.Server
{
    class MessengerService : IMessengerService
    {
        private readonly IEnumerable<UpdateType> _allowedUpdates = new []{UpdateType.Message};
        private readonly TelegramBotClient _client;
        private readonly IOptions<BotOptions> _options;
        private readonly ILogger<MessengerService> _logger;
        private int _lastUpdateId = 0;

        public MessengerService(IOptions<BotOptions> options, ILogger<MessengerService> logger)
        {
            _client = new TelegramBotClient(options.Value.Telegram.ApiToken,
                new HttpToSocks5Proxy(options.Value.Telegram.Socks5.Hostname, options.Value.Telegram.Socks5.Port));
            _options = options;
            _logger = logger;
        }

        public async Task<long?> GetIncomingMessageChatId(CancellationToken cancellationToken)
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
                if (allowedUsernames.Contains(username))
                {
                    long chatId = message.Chat.Id;

                    _logger.LogInformation("Message received from chat {ChatId}", chatId);
                    return chatId;
                }

                _logger.LogWarning("Message {@Message} discarded after comparing with allowed usernames {@AllowedUsernames}.", 
                    message, allowedUsernames);

                await Task.Delay(timeout, cancellationToken);
            }

            return null;
        }

        public async Task SendOops(long chatId, CancellationToken cancellationToken)
        {
            try
            {
                await _client.SendTextMessageAsync(chatId, "Something went wrong. Try again.", cancellationToken: cancellationToken);
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

        public async Task SendSnapshot(Stream snapshot, long chatId, CancellationToken cancellationToken)
        {
            var photo = new InputOnlineFile(snapshot);

            try
            {
                await _client.SendPhotoAsync(chatId, photo, cancellationToken: cancellationToken);

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
    }
}
