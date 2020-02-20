using Andead.CameraBot.Server.Messaging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Andead.CameraBot.Server
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCoreServices(this IServiceCollection services, IConfigurationSection configuration)
        {
            services.AddOptions();
            services.Configure<BotOptions>(configuration);

            services.AddTransient<IMessengerService, MessengerService>();
            services.AddTransient<ICameraService, CameraService>();

            return services;
        }
    }
}
