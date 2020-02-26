using System.Threading;
using System.Threading.Tasks;
using Andead.CameraBot.Messaging;

namespace Andead.CameraBot.Interfaces
{
    public interface ISnapshotRequestHandler
    {
        Task Handle(SnapshotRequest request, CancellationToken cancellationToken);
    }
}