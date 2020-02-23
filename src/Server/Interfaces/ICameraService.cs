using System.Collections.Generic;
using System.Threading.Tasks;

namespace Andead.CameraBot.Server
{
    interface ICameraService
    {
        Task<Snapshot> GetSnapshot(string cameraName);
        Task<IEnumerable<string>> GetAvailableCameraNames();
    }
}
