﻿using Andead.CameraBot.Media;
using Andead.CameraBot.Messaging;
using Microsoft.Extensions.DependencyInjection;

namespace Andead.CameraBot
{
    public static class CameraBotBuilderExtensions
    {
        public static ICameraBotBuilder AddCoreServices(this ICameraBotBuilder builder)
        {
            builder.Services.AddOptions();
            builder.Services.AddTransient<ICameraService, CameraService>();
            builder.Services.AddHostedService<BotService>();

            return builder;
        }

        public static ICameraBotBuilder AddMessenger<TMessenger>(this ICameraBotBuilder builder)
            where TMessenger : class, IMessenger
        {
            builder.Services.AddSingleton<IMessenger, TMessenger>();
            return builder;
        }
    }
}