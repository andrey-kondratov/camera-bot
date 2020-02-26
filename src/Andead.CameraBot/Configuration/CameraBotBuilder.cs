using Microsoft.Extensions.DependencyInjection;

namespace Andead.CameraBot
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