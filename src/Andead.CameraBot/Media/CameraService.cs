using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Andead.CameraBot.Media
{
    internal class CameraService : ICameraService
    {
        private readonly HttpClient _client;
        private readonly ILogger<CameraService> _logger;
        private readonly IOptions<CameraBotOptions> _options;

        public CameraService(HttpClient client, IOptions<CameraBotOptions> options, ILogger<CameraService> logger)
        {
            _client = client;
            _options = options;
            _logger = logger;
        }

        public Task<IEnumerable<string>> GetNames()
        {
            return Task.FromResult(_options.Value.Cameras.Values.Select(value => value.Name));
        }

        public Task<Snapshot> GetSnapshot(string cameraName)
        {
            return GetSnapshotInternal(_options.Value.Cameras.Values.Single(options =>
                string.Equals(options.Name, cameraName, StringComparison.OrdinalIgnoreCase)));
        }

        private async Task<Snapshot> GetSnapshotInternal(CameraOptions camera)
        {
            try
            {
                return new Snapshot
                {
                    CameraName = camera.Name,
                    CameraUrl = camera.Url,
                    Stream = await _client.GetStreamAsync(camera.SnapshotUrl).ConfigureAwait(false)
                };
            }
            catch (TaskCanceledException)
            {
                // ignored
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Error getting snapshot from {@Camera}.", camera);
            }

            return Snapshot.Error("Oops. Something went wrong.");
        }
    }
}