using Microsoft.Extensions.DependencyInjection;

namespace CameraBot
{
    public interface ICameraBotBuilder
    {
        IServiceCollection Services { get; }
    }
}