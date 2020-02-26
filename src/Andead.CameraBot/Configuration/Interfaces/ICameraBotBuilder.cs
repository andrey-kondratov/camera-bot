using Microsoft.Extensions.DependencyInjection;

namespace Andead.CameraBot
{
    public interface ICameraBotBuilder
    {
        IServiceCollection Services { get; }
    }
}