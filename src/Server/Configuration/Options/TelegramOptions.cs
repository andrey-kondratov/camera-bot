using System;

namespace Andead.CameraBot.Server
{
    public class TelegramOptions
    {
        public Socks5Options Socks5 { get; set; } = new Socks5Options();
        public string ApiToken { get; set; }
        public UpdatesOptions Updates { get; set; } = new UpdatesOptions();
        public string[] AllowedUsernames { get; set; } = Array.Empty<string>();
    }
}
