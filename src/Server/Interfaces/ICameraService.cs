using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Andead.CameraBot.Server
{
    interface ICameraService
    {
        Task<Stream> GetSnapshot(string cameraId);
        Task<IEnumerable<string>> GetAvailableCameraIds();
    }
}
