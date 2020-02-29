using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Andead.CameraBot.Media;

namespace Andead.CameraBot.Messaging
{
    public interface IMessenger
    {
        Task<bool> Test(CancellationToken cancellationToken = default);
        Task Start(CancellationToken cancellationToken = default);
        Task Stop(CancellationToken cancellationToken = default);
        Task SendSnapshot(Snapshot snapshot, ISnapshotRequest request, IEnumerable<string> cameraNames, CancellationToken cancellationToken = default);
        Task SendGreeting(ISnapshotRequest request, IEnumerable<string> cameraNames, CancellationToken cancellationToken = default);
        Task Handle(IncomingRequest request, CancellationToken cancellationToken = default);
        event EventHandler<SnapshotRequestedEventArgs> SnapshotRequested;
    }
}