using System;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Sockets.Plugin;
using WebSocket.Portable.Net;

namespace WebSocket.Portable
{
    public class TcpConnection : TcpConnectionBase
    {
        public readonly TcpSocketClient _client;
        private readonly bool _isSecure;
        //private StreamReader _reader;

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
        //private StreamReader Reader
       // {
       //     get { return _reader ?? (_reader = new StreamReader(_client.ReadStream, Encoding.UTF8)); }
       // }

        public async Task ConnectAsync(string host, CancellationToken cancellationToken)
        {
            var port = host.Contains("wss") ? 443 : 80;
            try
            {
                await _client.ConnectAsync(host, port, IsSecure);
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
            return _client.ReadStream.ReadAsync(buffer, offset, length, cancellationToken);
        }


        /// <summary>
        /// Receives a line asynchronous.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public override Task<string> ReadLineAsync(CancellationToken cancellationToken)
        {
            return Task.Run(() =>
             {
                 var sb = new StringBuilder();
                 var b = 0;
                 do
                 {
                     b = _client.ReadStream.ReadByte();
                     var ch = Convert.ToChar(b);

                     switch (ch)
                     {
                         case '\r':
                             break;
                         case '\n':
                             return sb.ToString();
                         default:
                             sb.Append(ch);
                             break;
                     }

                 } while (b != 0);

                 return sb.ToString();
             }, cancellationToken);

            // return Reader.ReadLineAsync();
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
            return _client.WriteStream.WriteAsync(buffer, offset, length, cancellationToken);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                //  if (Reader != null)
                //       Reader.Dispose();
                if (_client != null)
                    _client.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
