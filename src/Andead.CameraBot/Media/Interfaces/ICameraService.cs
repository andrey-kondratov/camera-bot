using System.Collections.Generic;
using System.Threading.Tasks;

namespace Andead.CameraBot.Media
{
    public interface ICameraService
    {
        Task<Snapshot> GetSnapshot(string cameraName);
        Task<IEnumerable<string>> GetNames();
    }
}
