using System.Collections.Generic;
using System.Linq;

namespace Andead.CameraBot
{
    internal static class NodeOptionsExtensions
    {
        /// <summary>
        ///     Builds a tree of <see cref="Node" /> from the given root node <see cref="NodeOptions" />.
        /// </summary>
        /// <param name="nodeOptions">The root node options.</param>
        /// <returns>An instance of <see cref="Node" /> representing the root node.</returns>
        public static Node ToNode(this NodeOptions nodeOptions)
        {
            nodeOptions.Name = "..";

            return CreateNode(nodeOptions, parent: null);
        }

        private static Node CreateNode(NodeOptions options, Node parent)
        {
            IEnumerable<Node> children = options.Children.Any()
                ? new List<Node>()
                : Enumerable.Empty<Node>();

            string id = MakeId(options, parent);
            var node = new Node(id, options.Name, options.SnapshotUrl, options.Url, options.Website, parent, children);

            foreach (NodeOptions childOptions in options.Children)
            {
                Node childNode = CreateNode(childOptions, node);
                ((List<Node>)children).Add(childNode);
            }

            return node;
        }

        private static string MakeId(NodeOptions options, Node parent)
        {
            string path = $"{parent?.Id}{options.Name}";
            int hash = path.GetDeterministicHashCode();

            return hash.ToString();
        }
    }
}