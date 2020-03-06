using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace Andead.CameraBot
{
    internal class CameraRegistry : ICameraRegistry
    {
        private readonly Node _root;
        private readonly IReadOnlyDictionary<string, Node> _map;

        public CameraRegistry(IOptions<CameraBotOptions> options)
        {
            _root = options.Value.Root.ToNode();
            _map = _root.ToDictionary(node => node.Id);
        }

        public Task<Node> GetNode(string id, CancellationToken cancellationToken = default)
        {
            Node node = _map.TryGetValue(id, out Node value) ? value : null;
            return Task.FromResult(node);
        }

        public Task<Node> GetRootNode(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_root);
        }
    }
}