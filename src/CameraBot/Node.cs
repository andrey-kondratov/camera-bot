using System;
using System.Collections.Generic;

namespace CameraBot
{
    /// <summary>
    ///     Represents a node in the cameras hierarchy tree.
    /// </summary>
    public sealed class Node
    {
        internal Node(string id, string name, string snapshotUrl, string url, string website, Node parent, IEnumerable<Node> children)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Node name must not be empty", nameof(name));
            }

            Id = id;
            Name = name;
            SnapshotUrl = snapshotUrl ?? parent?.SnapshotUrl;
            Url = url ?? parent?.Url;
            Website = website ?? parent?.Website;
            Parent = parent;
            Children = children;
        }

        /// <summary>
        ///     The node's unique id in the tree.
        /// </summary>
        public string Id { get; }

        /// <summary>
        ///     The friendly name of the camera.
        /// </summary>
        public string Name { get; }

        /// <summary>
        ///     The URL from which the snapshot can be downloaded. 
        /// </summary>
        public string SnapshotUrl { get; }

        /// <summary>
        ///     The optional URL of a page that streams video from the camera.
        /// </summary>
        public string Url { get; }

        /// <summary>
        ///     The optional URL of a website the camera is associated with.
        /// </summary>
        public string Website { get; }

        /// <summary>
        ///     May contain children nodes, i.e., cameras if the current node is a group, subgroups etc.
        /// </summary>
        public IEnumerable<Node> Children { get; }

        /// <summary>
        ///     The parent node of the current one.
        /// </summary>
        public Node Parent { get; }
    }
}
