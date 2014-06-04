using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WebSocket.Portable.Net
{
    public class TcpConnection : TcpConnectionBase
    {
        private readonly TcpClient _client;
        private NetworkStream _stream;
        private StreamReader _reader;

        /// <summary>
        /// Initializes a new instance of the <see cref="TcpConnection"/> class.
        /// </summary>
        public TcpConnection()
        {
            _client = new TcpClient();
        }

        /// <summary>
        /// Receives a line asynchronous.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public override Task<string> ReadLineAsync(CancellationToken cancellationToken)
        {
            return this.Reader.ReadLineAsync();
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (!disposing)
                return;

            if (_stream != null)
                _stream.Dispose();
            _client.Close();
        }

        public async Task ConnectAsync(string host, int port, CancellationToken cancellationToken)
        {
            try
            {
                await _client.ConnectAsync(host, port);
            }
            catch (SocketException se)
            {
                throw new WebException(string.Format("Failed to connect to '{0}:{1}'", host, port), se);
            }
        }

        /// <summary>
        /// Gets a value indicating whether data is available.
        /// </summary>
        /// <value>
        ///   <c>true</c> if data is available; otherwise, <c>false</c>.
        /// </value>
        public override bool IsDataAvailable
        {
            get { return this.Stream.DataAvailable; }
        }

        /// <summary>
        /// Gets a value indicating whether this tcp connection is secure.
        /// </summary>
        /// <value>
        /// <c>true</c> if this tcp connection is secure; otherwise, <c>false</c>.
        /// </value>
        public override bool IsSecure
        {
            get { return false; }
        }

        /// <summary>
        /// Sends data asynchronous.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="length">The length.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public override Task WriteAsync(byte[] buffer, int offset, int length, CancellationToken cancellationToken)
        {
            return this.Stream.WriteAsync(buffer, offset, length, cancellationToken);
        }

        /// <summary>
        /// Receives data asynchronous.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="length">The length.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public override Task<int> ReadAsync(byte[] buffer, int offset, int length, CancellationToken cancellationToken)
        {
            return this.Stream.ReadAsync(buffer, offset, length, cancellationToken);
        }

        /// <summary>
        /// Gets the stream.
        /// </summary>
        /// <value>
        /// The stream.
        /// </value>
        private NetworkStream Stream
        {
            get { return _stream ?? (_stream = _client.GetStream()); }
        }

        /// <summary>
        /// Gets the reader.
        /// </summary>
        /// <value>
        /// The reader.
        /// </value>
        private StreamReader Reader
        {
            get { return _reader ?? (_reader = new StreamReader(this.Stream, Encoding.UTF8)); }
        }
    }

}
