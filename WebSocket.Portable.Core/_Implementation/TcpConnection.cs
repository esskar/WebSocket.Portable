using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Sockets.Plugin;

namespace WebSocket.Portable.Net
{
    public class TcpConnection : TcpConnectionBase
    {
        private readonly TcpSocketClient _client;
        private readonly bool _isSecure;
        private StreamReader _reader;
        private volatile Stream _readstream;
        private volatile Stream _writestream;

        /// <summary>
        /// Initializes a new instance of the <see cref="TcpConnection" /> class.
        /// </summary>
        /// <param name="useSsl">if set to <c>true</c> the connection is secured using SSL.</param>
        public TcpConnection(bool useSsl)
        {
            _isSecure = useSsl;
            _client = new TcpSocketClient();
        }

        /// <summary>
        /// Gets a value indicating whether this tcp connection is secure.
        /// </summary>
        /// <value>
        /// <c>true</c> if this tcp connection is secure; otherwise, <c>false</c>.
        /// </value>
        public override bool IsSecure
        {
            get { return _isSecure; }
        }

        /// <summary>
        /// Gets the reader.
        /// </summary>
        /// <value>
        /// The reader.
        /// </value>
        private StreamReader Reader
        {
            get { return _reader ?? (_reader = new StreamReader(_readstream, Encoding.UTF8)); }
        }

        public async Task ConnectAsync(string host, CancellationToken cancellationToken)
        {
            var port = host.Contains("wss") ? 443 : 80;
            try
            {
                await _client.ConnectAsync(host, port);
                _writestream = _client.WriteStream;
                _readstream = _client.ReadStream;
            }
            catch (Exception se)
            {
                throw new WebException(string.Format("Failed to connect to '{0}:{1}'", host, port), se);
            }
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
            return _readstream.ReadAsync(buffer, offset, length, cancellationToken);
        }

        /// <summary>
        /// Receives a line asynchronous.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public override Task<string> ReadLineAsync(CancellationToken cancellationToken)
        {
            return Reader.ReadLineAsync();
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
            return _writestream.WriteAsync(buffer, offset, length, cancellationToken);
        }
        
        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (Reader != null)
                    Reader.Dispose();

                if (_writestream != null)
                    _writestream.Dispose();
                if (_readstream != null)
                    _readstream.Dispose();
                _writestream = null;
                _readstream = null;
                _client.Dispose();

                // do not dispose _reader
            }
            base.Dispose(disposing);
        }
    }
}
