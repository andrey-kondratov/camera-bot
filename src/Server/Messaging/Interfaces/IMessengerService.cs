using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Andead.CameraBot.Server.Messaging
{
    interface IMessengerService
    {
        Task<bool> Test(CancellationToken cancellationToken);
        Task<SnapshotRequest> GetSnapshotRequest(CancellationToken cancellationToken);
        Task SendOops(long chatId, CancellationToken cancellationToken);
        Task SendSnapshot(Stream snapshot, long chatId, CancellationToken cancellationToken);
    }
}
