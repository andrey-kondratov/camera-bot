using Andead.CameraBot.Messaging;

namespace Andead.CameraBot.Telegram
{
    public class SnapshotRequest : ISnapshotRequest
    {
        public long ChatId { get; set; }
        public string Text { get; set; }
    }
}