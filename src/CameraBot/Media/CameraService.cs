using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace CameraBot.Media
{
    internal class CameraService : ICameraService
    {
        private readonly HttpClient _client;
        private readonly ILogger<CameraService> _logger;

        public CameraService(HttpClient client, ILogger<CameraService> logger)
        {
            _client = client;
            _logger = logger;
        }

        public async Task<Snapshot> GetSnapshot(Node node, CancellationToken cancellationToken = default)
        {
            try
            {
                var snapshot = new Snapshot
                {
                    Node = node
                };

                if (!string.IsNullOrEmpty(node.SnapshotUrl))
                {
                    snapshot.Stream = await _client.GetStreamAsync(node.SnapshotUrl).ConfigureAwait(false);
                }
                else
                {
                    snapshot.Success = false;
                }

                return snapshot;
            }
            catch (TaskCanceledException)
            {
                // ignored
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Error getting snapshot from {@Camera}.", node);
            }

            return Snapshot.Error("Oops. Something went wrong.");
        }
    }
}