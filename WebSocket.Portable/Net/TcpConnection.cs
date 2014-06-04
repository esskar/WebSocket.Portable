using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WebSocket.Portable.Net
{
    public class TcpConnection : TcpConnectionBase
    {
        private readonly object _sync = new object();
        private readonly TcpClient _client;
        private readonly bool _isSecure;
        private volatile Stream _stream;
        private StreamReader _reader;
        private string _host;

        /// <summary>
        /// Initializes a new instance of the <see cref="TcpConnection" /> class.
        /// </summary>
        /// <param name="useSsl">if set to <c>true</c> the connection is secured using SSL.</param>
        public TcpConnection(bool useSsl)
        {
            _isSecure = useSsl;
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
            if (disposing)
            {
                if (_stream != null)
                    _stream.Dispose();
                _client.Close();

                // do not dispose _reader
            }
            base.Dispose(disposing);            
        }

        public async Task ConnectAsync(string host, int port, CancellationToken cancellationToken)
        {
            try
            {
                _host = host;
                await _client.ConnectAsync(host, port);
            }
            catch (SocketException se)
            {
                throw new WebException(string.Format("Failed to connect to '{0}:{1}'", host, port), se);
            }
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
        private Stream Stream
        {
            get
            {
                if (_stream != null)
                    return _stream;

                lock (_sync)
                {
                    if (_stream != null)
                        return _stream;

                    Stream stream = _client.GetStream();
                    if (_isSecure)
                    {
                        var sslStream = new SslStream(stream);
                        sslStream.AuthenticateAsClient(_host);                        
                        stream = sslStream;
                    }
                    _stream = stream;                    
                }
                return _stream;
            }
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
