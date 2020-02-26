using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Andead.CameraBot.Media;
using Andead.CameraBot.Messaging;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Andead.CameraBot
{
    public class Worker : BackgroundService
    {
        private readonly ICameraService _camera;
        private readonly IMessenger _messenger;
        private readonly CameraBotOptions _options;

        public Worker(ICameraService cameraService, IMessenger messenger, IOptions<CameraBotOptions> options)
        {
            _options = options.Value;
            _camera = cameraService;
            _messenger = messenger;
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
                await Task.Delay(_options.PollingInterval, stoppingToken);

                SnapshotRequest request = await _messenger.GetSnapshotRequest(stoppingToken);
                if (request == null)
                {
                    continue;
                }

                IEnumerable<string> cameraIds = await _camera.GetAvailableCameraNames();

                using Snapshot snapshot = await _camera.GetSnapshot(request.Text);
                if (snapshot == null)
                {
                    await _messenger.SendGreeting(request.ChatId, cameraIds, stoppingToken);
                    continue;
                }

                await _messenger.SendSnapshot(snapshot, request.ChatId, cameraIds, stoppingToken);
            }
        }
    }
}