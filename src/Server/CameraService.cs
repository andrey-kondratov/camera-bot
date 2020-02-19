using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Andead.CameraBot.Server
{
    internal class CameraService : ICameraService
    {
        private readonly IOptions<BotOptions> _options;
        private readonly ILogger<CameraService> _logger;
        private readonly HttpClient _client;

        public CameraService(IOptions<BotOptions> options, ILogger<CameraService> logger)
        {
            _options = options;
            _logger = logger;
            _client = new HttpClient();
        }

        public async Task<Stream> GetSnapshot()
        {
            string url = _options.Value.Camera.SnapshotUrl;

            try
            {
                return await _client.GetStreamAsync(url);
            }
            catch (TaskCanceledException)
            {
                // ignored
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Error getting snapshot from {SnapshotUrl}.", url);
            }

            return null;
        }
    }
}
