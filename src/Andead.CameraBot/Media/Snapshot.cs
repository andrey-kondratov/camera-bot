using System;
using System.IO;

namespace Andead.CameraBot.Media
{
    public sealed class Snapshot : IDisposable
    {
        public bool Success { get; set; } = true;
        public string Message { get; set; }
        public DateTime TakenUtc { get; set; } = DateTime.UtcNow;
        public string CameraName { get; set; }
        public string CameraUrl { get; set; }
        public Stream Stream { get; set; }

        internal static Snapshot Error(string message) => new Snapshot { Success = false, Message = message};

        public void Dispose()
        {
            Stream?.Dispose();
        }
    }
}
