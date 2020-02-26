using System;
using System.Threading;
using System.Threading.Tasks;
using Andead.CameraBot.Interfaces;
using Andead.CameraBot.Messaging;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Andead.CameraBot
{
    public class PollingService : BackgroundService
    {
        private readonly ISnapshotRequestHandler _handler;
        private readonly ILogger<PollingService> _logger;
        private readonly IMessenger _messenger;
        private readonly CameraBotOptions _options;

        public PollingService(IMessenger messenger, ISnapshotRequestHandler handler, IOptions<CameraBotOptions> options,
            ILogger<PollingService> logger)
        {
            _options = options.Value;
            _messenger = messenger;
            _handler = handler;
            _logger = logger;
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

                try
                {
                    await _handler.Handle(request, stoppingToken);
                }
                catch (Exception exception)
                {
                    _logger.LogError(exception, "Unhandled exception handling snapshot request: {@SnapshotRequest}",
                        request);
                }
            }
        }
    }
}