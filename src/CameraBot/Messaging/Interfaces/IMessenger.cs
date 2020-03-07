using System;
using System.Threading;
using System.Threading.Tasks;
using CameraBot.Media;

namespace CameraBot.Messaging
{
    public interface IMessenger
    {
        Task<bool> Test(CancellationToken cancellationToken = default);
        Task Start(CancellationToken cancellationToken = default);
        Task Stop(CancellationToken cancellationToken = default);
        Task Handle(IncomingRequest request, CancellationToken cancellationToken = default);
        event EventHandler<SnapshotRequestedEventArgs> SnapshotRequested;

        /// <summary>
        ///     Updates the navigation controls for the current node.
        /// </summary>
        /// <param name="node">The current node.</param>
        /// <param name="alert">Optional alert message to show the user.</param>
        Task Navigate(Node node, ISnapshotRequest request, string alert = null, CancellationToken cancellationToken = default);

        /// <summary>
        ///     Replies to <paramref name="request" /> with a <paramref name="snapshot" /> and updates 
        ///     the navigation controls for <paramref name="nodeToNavigate" />.
        /// </summary>
        /// <param name="snapshot">The snapshot.</param>
        /// <param name="request">The snapshot request.</param>
        /// <param name="nodeToNavigate">The current node.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        Task SendSnapshotAndNavigate(Snapshot snapshot, ISnapshotRequest request, Node nodeToNavigate,
            CancellationToken cancellationToken = default);
    }
}