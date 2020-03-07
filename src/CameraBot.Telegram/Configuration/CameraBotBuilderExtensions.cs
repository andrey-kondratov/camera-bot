using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CameraBot.Telegram
{
    public static class CameraBotBuilderExtensions
    {
        public static ICameraBotBuilder AddTelegram(this ICameraBotBuilder builder)
        {
            builder.Services.AddOptions();
            return builder.AddMessenger<Messenger>();
        }

        public static ICameraBotBuilder AddTelegram(this ICameraBotBuilder builder, Action<TelegramOptions> setupAction)
        {
            builder.Services.Configure(setupAction);
            return builder.AddTelegram();
        }

        public static ICameraBotBuilder AddTelegram(this ICameraBotBuilder builder, IConfiguration configuration)
        {
            builder.Services.Configure<TelegramOptions>(configuration);
            return builder.AddTelegram();
        }
    }
}