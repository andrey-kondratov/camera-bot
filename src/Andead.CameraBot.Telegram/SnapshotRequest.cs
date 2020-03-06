using Andead.CameraBot.Messaging;
using Telegram.Bot.Types;

namespace Andead.CameraBot.Telegram
{
    public class SnapshotRequest : ISnapshotRequest
    {
        public string Id { get; set; }
        public CallbackQuery Query { get; set; }
    }
}