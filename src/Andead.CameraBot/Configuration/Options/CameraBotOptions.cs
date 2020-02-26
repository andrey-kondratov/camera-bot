using System.Collections.Generic;

namespace Andead.CameraBot
{
    public class CameraBotOptions
    {
        public Dictionary<string, CameraOptions> Cameras { get; set; } = new Dictionary<string, CameraOptions>();
        public int PollingInterval { get; set; } = 1000;
        public string DateTimeFormat { get; set; } = "yyyy-MM-dd HH:mm:ss";
        public int HoursOffset { get; set; } = 3;
    }
}
