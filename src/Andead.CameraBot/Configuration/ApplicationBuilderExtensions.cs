using Microsoft.AspNetCore.Builder;

namespace Andead.CameraBot
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseCameraBot(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<WebhooksMiddleware>();
        }
    }
}