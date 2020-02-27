using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Andead.CameraBot.Media;

namespace Andead.CameraBot.Messaging
{
    public interface IMessenger
    {
        Task<bool> Test(CancellationToken cancellationToken);
        void StartReceiving(CancellationToken cancellationToken);
        void StopReceiving(CancellationToken cancellationToken);

        Task SendSnapshot(Snapshot snapshot, ISnapshotRequest request, IEnumerable<string> cameraNames,
            CancellationToken cancellationToken);

        Task SendGreeting(ISnapshotRequest request, IEnumerable<string> cameraNames,
            CancellationToken cancellationToken);

        event EventHandler<SnapshotRequestedEventArgs> SnapshotRequested;
    }
}