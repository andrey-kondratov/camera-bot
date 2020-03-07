using System.Threading;
using System.Threading.Tasks;

namespace CameraBot.Media
{
    public interface ICameraService
    {
        Task<Snapshot> GetSnapshot(Node node, CancellationToken cancellationToken = default);
    }
}
