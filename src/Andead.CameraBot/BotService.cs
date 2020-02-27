using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Andead.CameraBot.Media;
using Andead.CameraBot.Messaging;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Andead.CameraBot
{
    public class BotService : IHostedService
    {
        private readonly ILogger<BotService> _logger;
        private readonly ICameraService _cameraService;
        private readonly IMessenger _messenger;

        public BotService(ICameraService cameraService, IMessenger messenger, ILogger<BotService> logger)
        {
            _cameraService = cameraService;
            _messenger = messenger;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting bot");
            _logger.LogInformation("Testing {MessengerType}", _messenger.GetType().FullName);

            bool testOk = await _messenger.Test(cancellationToken);
            if (!testOk)
            {
                _logger.LogWarning("Messenger test failed.");
                return;
            }
            
            _logger.LogInformation("Messenger test successful.");

            _messenger.SnapshotRequested += MessengerOnSnapshotRequested;
            _messenger.StartReceiving(cancellationToken);

            _logger.LogInformation("Bot started.");
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping bot");

            _messenger.StopReceiving(cancellationToken);
            _messenger.SnapshotRequested -= MessengerOnSnapshotRequested;

            _logger.LogInformation("Bot stopped.");

            return Task.CompletedTask;
        }

        private void MessengerOnSnapshotRequested(object sender, SnapshotRequestedEventArgs e)
        {
            try
            {
                Handle(e.SnapshotRequest, e.CancellationToken).GetAwaiter().GetResult();
            }
            catch (TaskCanceledException)
            {
                _logger.LogInformation("Canceled handling snapshot request {@SnapshotRequest}", e.SnapshotRequest);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Unhandled exception handling snapshot request: {@SnapshotRequest}",
                    e.SnapshotRequest);
            }
        }

        public async Task Handle(ISnapshotRequest request, CancellationToken cancellationToken)
        {
            IEnumerable<string> cameraIds = await _cameraService.GetAvailableCameraNames();

            using Snapshot snapshot = await _cameraService.GetSnapshot(request.Text);
            if (snapshot == null)
            {
                await _messenger.SendGreeting(request, cameraIds, cancellationToken);
                return;
            }

            await _messenger.SendSnapshot(snapshot, request, cameraIds, cancellationToken);
        }
    }
}