using System;
using Andead.CameraBot.Interfaces;
using Andead.CameraBot.Media;
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
            builder.Services.AddTransient<ISnapshotRequestHandler, SnapshotRequestHandler>();

            return builder;
        }

        public static ICameraBotBuilder AddMessenger<TMessenger>(this ICameraBotBuilder builder)
            where TMessenger : class, IMessenger
        {
            builder.Services.AddTransient<IMessenger, TMessenger>();
            return builder;
        }

        public static ICameraBotBuilder AddPolling(this ICameraBotBuilder builder)
        {
            builder.Services.AddHostedService<PollingService>();
            return builder;
        }

        public static ICameraBotBuilder AddWebhooks(this ICameraBotBuilder builder)
        {
            throw new NotSupportedException();
        }
    }
}