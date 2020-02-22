﻿using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        public Task<IEnumerable<string>> GetAvailableCameraNames()
        {
            return Task.FromResult(_options.Value.Cameras.Values.Select(value => value.Name));
        }

        public async Task<Stream> GetSnapshot(string cameraName)
        {
            CameraOptions camera = _options.Value.Cameras.Values
                .FirstOrDefault(c => string.Equals(cameraName, c.Name, StringComparison.OrdinalIgnoreCase));
            if (camera == null)
            {
                _logger.LogWarning("Camera with name {CameraName} was not found.", cameraName);
                return null;
            }

            string url = camera.SnapshotUrl;

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
