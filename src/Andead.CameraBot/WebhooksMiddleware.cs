using System;
using System.Threading;
using System.Threading.Tasks;
using Andead.CameraBot.Messaging;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Andead.CameraBot
{
    internal sealed class WebhooksMiddleware
    {
        private readonly ILogger<WebhooksMiddleware> _logger;
        private readonly RequestDelegate _next;
        private readonly IMessenger _messenger;

        public WebhooksMiddleware(RequestDelegate next, IMessenger messenger, ILogger<WebhooksMiddleware> logger)
        {
            _next = next;
            _messenger = messenger;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            var request = new IncomingRequest(context.Request);
            CancellationToken cancellationToken = context.RequestAborted;

            try
            {
                await _messenger.Handle(request, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Unhandled exception in messenger {Messenger}.", _messenger);
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                return;
            }

            if (request.Handled)
            {
                context.Response.StatusCode = StatusCodes.Status200OK;
                return;
            }

            await _next.Invoke(context).ConfigureAwait(false);
        }
    }
}