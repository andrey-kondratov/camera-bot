using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Andead.CameraBot.Interfaces;
using Andead.CameraBot.Media;
using Andead.CameraBot.Messaging;

namespace Andead.CameraBot
{
    public class SnapshotRequestHandler : ISnapshotRequestHandler
    {
        private readonly ICameraService _camera;
        private readonly IMessenger _messenger;

        public SnapshotRequestHandler(ICameraService cameraService, IMessenger messenger)
        {
            _camera = cameraService;
            _messenger = messenger;
        }

        public async Task Handle(SnapshotRequest request, CancellationToken cancellationToken)
        {
            IEnumerable<string> cameraIds = await _camera.GetAvailableCameraNames();

            using Snapshot snapshot = await _camera.GetSnapshot(request.Text);
            if (snapshot == null)
            {
                await _messenger.SendGreeting(request.ChatId, cameraIds, cancellationToken);
                return;
            }

            await _messenger.SendSnapshot(snapshot, request.ChatId, cameraIds, cancellationToken);
        }
    }
}