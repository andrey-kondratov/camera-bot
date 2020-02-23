using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Andead.CameraBot.Server.Messaging
{
    interface IMessengerService
    {
        Task<bool> Test(CancellationToken cancellationToken);
        Task<SnapshotRequest> GetSnapshotRequest(CancellationToken cancellationToken);
        Task SendOops(long chatId, IEnumerable<string> cameraNames, CancellationToken cancellationToken);
        Task SendSnapshot(Snapshot snapshot, long chatId, IEnumerable<string> cameraNames, CancellationToken cancellationToken);
    }
}
