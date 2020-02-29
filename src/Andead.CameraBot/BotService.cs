using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Andead.CameraBot.Media;
using Andead.CameraBot.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Andead.CameraBot
{
    internal class BotService : IHostedService
    {
        private readonly ILogger<BotService> _logger;
        private readonly IMessenger _messenger;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public BotService(IServiceScopeFactory serviceScopeFactory, IMessenger messenger, ILogger<BotService> logger)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _messenger = messenger;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting bot");
            _logger.LogInformation("Testing {MessengerType}", _messenger.GetType().FullName);

            bool testOk = await _messenger.Test(cancellationToken).ConfigureAwait(false);
            if (!testOk)
            {
                _logger.LogWarning("Messenger test failed.");
                return;
            }

            _logger.LogInformation("Messenger test successful.");

            _messenger.SnapshotRequested += MessengerOnSnapshotRequested;
            await _messenger.Start(cancellationToken).ConfigureAwait(false);

            _logger.LogInformation("Bot started.");
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping bot");

            await _messenger.Stop(cancellationToken).ConfigureAwait(false);
            _messenger.SnapshotRequested -= MessengerOnSnapshotRequested;

            _logger.LogInformation("Bot stopped.");
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
            using IServiceScope scope = _serviceScopeFactory.CreateScope();
            var cameraService = scope.ServiceProvider.GetService<ICameraService>();

            IEnumerable<string> cameraNames = (await cameraService.GetNames()
                .ConfigureAwait(false)).ToList();

            if (cameraNames.Any(request.Text.Equals))
            {
                using Snapshot snapshot = await cameraService.GetSnapshot(request.Text).ConfigureAwait(false);
                await _messenger.SendSnapshot(snapshot, request, cameraNames, cancellationToken).ConfigureAwait(false);
                return;
            }

            await _messenger.SendGreeting(request, cameraNames, cancellationToken).ConfigureAwait(false);
        }
    }
}