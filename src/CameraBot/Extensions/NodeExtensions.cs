using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace CameraBot
{
    public static class NodeExtensions
    {
        /// <summary>
        ///     Recursively builds a dictionary of tree nodes.
        /// </summary>
        /// <param name="root">The root node.</param>
        /// <param name="keySelector">The key selector.</param>
        public static IReadOnlyDictionary<TKey, Node> ToDictionary<TKey>(this Node root, Func<Node, TKey> keySelector)
        {
            var map = new Dictionary<TKey, Node>();

            void Add(Node node)
            {
                TKey key = keySelector(node);
                map.Add(key, node);

                foreach (Node child in node.Children)
                {
                    Add(child);
                }
            }

            Add(root);

            return new ReadOnlyDictionary<TKey, Node>(map);
        }

        /// <summary>
        ///     Returns <see langword="true" /> if <paramref name="node" /> has no children.
        /// </summary>
        /// <param name="node">The node.</param>
        public static bool IsLeaf(this Node node)
        {
            return !node.Children.Any();
        }

        /// <summary>
        ///     Returns <see langword="true" /> if <paramref name="node" /> has the root as its parent.
        /// </summary>
        /// <param name="node">The node.</param>
        public static bool IsRootChild(this Node node)
        {
            return node.Parent.Parent == null;
        }
    }
}