using System;
using System.Net.Http;
using CameraBot.Media;
using CameraBot.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Extensions.Http;
using Polly.Retry;
using Polly.Timeout;

namespace CameraBot
{
    public static class CameraBotBuilderExtensions
    {
        internal static ICameraBotBuilder AddCoreServices(this ICameraBotBuilder builder)
        {
            builder.Services.AddOptions();
            builder.Services.AddHostedService<BotService>();
            builder.Services.AddSingleton<ICameraRegistry, CameraRegistry>();

            return builder;
        }

        internal static ICameraBotBuilder AddHttpClient(this ICameraBotBuilder builder)
        {
            using ServiceProvider serviceProvider = builder.Services.BuildServiceProvider(false);
            CameraBotOptions options = serviceProvider.GetRequiredService<IOptions<CameraBotOptions>>().Value;

            AsyncRetryPolicy<HttpResponseMessage> retryPolicy = HttpPolicyExtensions
                .HandleTransientHttpError()
                .Or<TimeoutRejectedException>()
                .RetryAsync(options.RetryCount);

            AsyncTimeoutPolicy<HttpResponseMessage> timeoutPolicy =
                Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromMilliseconds(options.TimeoutMilliseconds));

            builder.Services
                .AddHttpClient<ICameraService, CameraService>()
                .AddPolicyHandler(retryPolicy)
                .AddPolicyHandler(timeoutPolicy);

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