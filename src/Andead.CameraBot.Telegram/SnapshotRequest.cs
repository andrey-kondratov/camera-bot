using Andead.CameraBot.Messaging;
using Telegram.Bot.Types;

namespace Andead.CameraBot.Telegram
{
    public class SnapshotRequest : ISnapshotRequest
    {
        public string Path { get; set; }
        public CallbackQuery Query { get; set; }
    }
}