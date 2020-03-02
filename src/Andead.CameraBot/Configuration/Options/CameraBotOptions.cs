using System.Collections.Generic;

namespace Andead.CameraBot
{
    public class CameraBotOptions
    {
        public Dictionary<string, CameraOptions> Cameras { get; set; } = new Dictionary<string, CameraOptions>();
        public int RetryCount { get; set; } = 3;
        public int TimeoutMilliseconds { get; set; } = 1000;
    }
}
