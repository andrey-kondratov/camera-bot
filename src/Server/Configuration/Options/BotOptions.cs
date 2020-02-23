using System.Collections.Generic;

namespace Andead.CameraBot.Server
{
    public class BotOptions
    {
        public TelegramOptions Telegram { get; set; } = new TelegramOptions();
        public Dictionary<string, CameraOptions> Cameras { get; set; } = new Dictionary<string, CameraOptions>();
        public int PollingInterval { get; set; } = 1000;
        public string DateTimeFormat { get; set; } = "yyyy-MM-dd HH:mm:ss";
        public int HoursOffset { get; set; } = 3;
    }
}
