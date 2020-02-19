namespace Andead.CameraBot.Server
{
    public class BotOptions
    {
        public TelegramOptions Telegram { get; set; } = new TelegramOptions();
        public CameraOptions Camera { get; set; } = new CameraOptions();
        public int PollingInterval { get; set; } = 1000;
    }
}
