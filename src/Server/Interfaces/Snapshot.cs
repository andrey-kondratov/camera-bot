using System;
using System.IO;

namespace Andead.CameraBot.Server
{
    public sealed class Snapshot : IDisposable
    {
        public DateTime TakenUtc { get; set; } = DateTime.UtcNow;
        public string CameraName { get; set; }
        public string CameraUrl { get; set; }
        public Stream Stream { get; set; }

        public void Dispose()
        {
            Stream?.Dispose();
        }
    }
}
