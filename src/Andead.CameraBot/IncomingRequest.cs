using Microsoft.AspNetCore.Http;

namespace Andead.CameraBot
{
    public sealed class IncomingRequest
    {
        internal IncomingRequest(HttpRequest httpRequest)
        {
            HttpRequest = httpRequest;
        }

        public HttpRequest HttpRequest { get; }
        public bool Handled { get; set; }
    }
}