using CameraBot.Messaging;
using Telegram.Bot.Types;

namespace CameraBot.Telegram
{
    public class SnapshotRequest : ISnapshotRequest
    {
        public string Id { get; set; }
        public CallbackQuery Query { get; set; }
    }
}