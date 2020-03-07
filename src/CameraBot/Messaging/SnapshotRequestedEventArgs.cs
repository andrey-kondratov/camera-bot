using System;
using System.Threading;

namespace CameraBot.Messaging
{
    public class SnapshotRequestedEventArgs : EventArgs
    {
        public SnapshotRequestedEventArgs(ISnapshotRequest snapshotRequest, CancellationToken cancellationToken = default)
        {
            SnapshotRequest = snapshotRequest;
            CancellationToken = cancellationToken;
        }

        public ISnapshotRequest SnapshotRequest { get; }

        public CancellationToken CancellationToken { get; }
    }
}