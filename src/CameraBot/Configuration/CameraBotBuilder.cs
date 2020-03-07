using Microsoft.Extensions.DependencyInjection;

namespace CameraBot
{
    internal class CameraBotBuilder : ICameraBotBuilder
    {
        public CameraBotBuilder(IServiceCollection services)
        {
            Services = services;
        }

        public IServiceCollection Services { get; }
    }
}