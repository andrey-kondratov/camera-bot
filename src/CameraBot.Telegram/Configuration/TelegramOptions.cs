using System;

namespace CameraBot.Telegram
{
    public class TelegramOptions
    {
        public Socks5Options Socks5 { get; set; } = new Socks5Options();
        public string ApiToken { get; set; }
        public string[] AllowedUsernames { get; set; } = Array.Empty<string>();
        public string DateTimeFormat { get; set; } = "yyyy-MM-dd HH:mm:ss";
        public int HoursOffset { get; set; } = 3;
        public WebhookOptions Webhook { get; set; } = new WebhookOptions();
        public FeedbackOptions Feedback { get; set; } = new FeedbackOptions();
    }
}
