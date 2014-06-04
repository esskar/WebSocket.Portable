using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Networking;
using Windows.Networking.Sockets;

namespace WebSocket.Portable.Net
{
    public class TcpConnection : TcpConnectionBase
    {
        private readonly bool _isSecure;
        private readonly StreamSocket _streamSocket;
        private Stream _outputStream;
        private Stream _inputStream;
        private StreamReader _reader;

        public TcpConnection(bool useSsl)
        {
            _isSecure = useSsl;
            _streamSocket = new StreamSocket();
        }

        public override bool IsSecure
        {
            get { return _isSecure; }
        }

        public async Task ConnectAsync(string host, int port, CancellationToken cancellationToken)
        {
            try
            {
                var hostName = new HostName(host);
                await _streamSocket.ConnectAsync(hostName, port.ToString(), this.IsSecure ? SocketProtectionLevel.Ssl : SocketProtectionLevel.PlainSocket);

                _outputStream = _streamSocket.OutputStream.AsStreamForWrite();
                _inputStream = _streamSocket.InputStream.AsStreamForRead();
            }
            catch (Exception se)
            {
                throw new WebException(string.Format("Failed to connect to '{0}:{1}'", host, port), se);
            }
        }

        public override Task<string> ReadLineAsync(CancellationToken cancellationToken)
        {
            return this.Reader.ReadLineAsync();
        }

        public override Task<int> ReadAsync(byte[] buffer, int offset, int length, CancellationToken cancellationToken)
        {
            return _inputStream.ReadAsync(buffer, offset, length, cancellationToken);
        }

        public override Task WriteAsync(byte[] buffer, int offset, int length, CancellationToken cancellationToken)
        {
            return _outputStream.WriteAsync(buffer, offset, offset, cancellationToken);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _outputStream.Dispose();
                _inputStream.Dispose();                
                _streamSocket.Dispose();

                // do not dispose _reader
            }

            base.Dispose(disposing);
        }

        private StreamReader Reader
        {
            get { return _reader ?? (_reader = new StreamReader(_inputStream, Encoding.UTF8)); }
        }
    }
}
