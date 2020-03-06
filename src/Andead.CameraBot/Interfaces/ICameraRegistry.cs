using System.Threading;
using System.Threading.Tasks;

namespace Andead.CameraBot
{
    /// <summary>
    ///     Supports getting camera tree nodes by paths.
    /// </summary>
    public interface ICameraRegistry
    {
        /// <summary>
        ///     Returns a node by its id.
        /// </summary>
        /// <param name="id">The unique id.</param>
        /// <returns>Instance of <see cref="Node" /> if node found or <see langword="null" /> otherwise.</returns>
        Task<Node> GetNode(string id, CancellationToken cancellationToken = default);

        /// <summary>
        ///     Returns the root node.
        /// </summary>
        /// <returns>Instance of the root <see cref="Node" />.</returns>
        Task<Node> GetRootNode(CancellationToken cancellationToken = default);
    }
}