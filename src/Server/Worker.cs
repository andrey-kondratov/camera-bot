using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Andead.CameraBot.Server.Messaging;
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

                SnapshotRequest request = await _messenger.GetSnapshotRequest(stoppingToken);
                if (request == null)
                {
                    continue;
                }

                IEnumerable<string> cameraIds = await _camera.GetAvailableCameraIds();

                using Stream snapshot = await _camera.GetSnapshot(request.Text);
                if (snapshot == null)
                {
                    await _messenger.SendOops(request.ChatId, cameraIds, stoppingToken);
                    continue;
                }

                await _messenger.SendSnapshot(snapshot, request.ChatId, cameraIds, stoppingToken);
            }
        }
    }
}
