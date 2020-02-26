using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Andead.CameraBot.Media;

namespace Andead.CameraBot.Messaging
{
    public interface IMessenger
    {
        Task<bool> Test(CancellationToken cancellationToken);
        Task<SnapshotRequest> GetSnapshotRequest(CancellationToken cancellationToken);
        Task SendSnapshot(Snapshot snapshot, long chatId, IEnumerable<string> cameraNames, CancellationToken cancellationToken);
        Task SendGreeting(long chatId, IEnumerable<string> cameraNames, CancellationToken cancellationToken);
    }
}
