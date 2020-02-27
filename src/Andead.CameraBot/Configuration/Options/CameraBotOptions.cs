using System.Collections.Generic;

namespace Andead.CameraBot
{
    public class CameraBotOptions
    {
        public Dictionary<string, CameraOptions> Cameras { get; set; } = new Dictionary<string, CameraOptions>();
    }
}
