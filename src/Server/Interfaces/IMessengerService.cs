using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Andead.CameraBot.Server
{
    interface IMessengerService
    {
        Task<bool> Test(CancellationToken cancellationToken);
        Task<long?> GetIncomingMessageChatId(CancellationToken cancellationToken);
        Task SendOops(long chatId, CancellationToken cancellationToken);
        Task SendSnapshot(Stream snapshot, long chatId, CancellationToken cancellationToken);
    }
}
