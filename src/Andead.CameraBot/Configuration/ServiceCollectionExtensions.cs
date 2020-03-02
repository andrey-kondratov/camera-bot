using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Andead.CameraBot
{
    public static class ServiceCollectionExtensions
    {
        internal static ICameraBotBuilder AddCameraBotBuilder(this IServiceCollection services)
        {
            return new CameraBotBuilder(services);
        }

        public static ICameraBotBuilder AddCameraBot(this IServiceCollection services)
        {
            ICameraBotBuilder builder = services.AddCameraBotBuilder();
            
            builder
                .AddCoreServices()
                .AddHttpClient();

            return builder;
        }

        public static ICameraBotBuilder AddCameraBot(this IServiceCollection services,
            IConfigurationSection configuration)
        {
            services.Configure<CameraBotOptions>(configuration);
            return services.AddCameraBot();
        }

        public static ICameraBotBuilder AddCameraBot(this IServiceCollection services,
            Action<CameraBotOptions> setupAction)
        {
            services.Configure(setupAction);
            return services.AddCameraBot();
        }
    }
}