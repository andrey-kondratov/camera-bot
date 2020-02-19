using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Andead.CameraBot.Server
{
    internal class Worker : BackgroundService
    {
        private readonly BotOptions _options;
        private readonly ICameraService _camera;
        private readonly IMessengerService _messenger;

        public Worker(ICameraService cameraService, IMessengerService messengerService, IOptions<BotOptions> options)
        {
            _options = options.Value;
            _camera = cameraService;
            _messenger = messengerService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            bool testOk = await _messenger.Test(stoppingToken);
            if (!testOk)
            {
                return;
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(_options.PollingInterval);

                long? chatId = await _messenger.GetIncomingMessageChatId(stoppingToken);
                if (chatId == null)
                {
                    continue;
                }

                using Stream snapshot = await _camera.GetSnapshot();
                if (snapshot == null)
                {
                    await _messenger.SendOops(chatId.Value, stoppingToken);
                    continue;
                }

                await _messenger.SendSnapshot(snapshot, chatId.Value, stoppingToken);
            }
        }
    }
}
