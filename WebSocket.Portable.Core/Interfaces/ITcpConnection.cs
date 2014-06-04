using System.Threading;
using System.Threading.Tasks;

namespace WebSocket.Portable.Interfaces
{
    public interface ITcpConnection : IDataLayer
    {
        /// <summary>
        /// Gets a value indicating whether data is available.
        /// </summary>
        /// <value>
        /// <c>true</c> if data is available; otherwise, <c>false</c>.
        /// </value>
        bool IsDataAvailable { get; }

        /// <summary>
        /// Gets a value indicating whether this tcp connection is secure.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this tcp connection is secure; otherwise, <c>false</c>.
        /// </value>
        bool IsSecure { get; }

        /// <summary>
        /// Reads a line asynchronous.
        /// </summary>
        /// <returns></returns>
        Task<string> ReadLineAsync();

        /// <summary>
        /// Reads a line asynchronous.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task<string> ReadLineAsync(CancellationToken cancellationToken);
    }
}
