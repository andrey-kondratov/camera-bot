using System;

namespace Andead.CameraBot
{
    internal sealed class Options
    {
        public string Socks5Hostname { get; set; }
        public int Socks5Port { get; set; } = 9100;
        public string TelegramBotApiKey { get; set; }
        public int TelegramPollingTimeoutSeconds { get; set; } = 1;
        public int TelegramPollingLimit { get; set; } = 1;
        public string[] TelegramAllowedUsernames { get; set; } = Array.Empty<string>();
        public string CameraImageUrl { get; set; }
        public int IntervalMs { get; set; } = 1000;
    }
}
