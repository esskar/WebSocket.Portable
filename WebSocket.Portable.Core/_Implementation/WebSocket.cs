using System.Threading;
using System.Threading.Tasks;
using WebSocket.Portable.Interfaces;

namespace WebSocket.Portable
{
    public class WebSocket : WebSocketBase
    {
        internal TcpConnection InnerConnection;
        protected override async Task<ITcpConnection> ConnectAsync(string host, int port, bool useSsl, CancellationToken cancellationToken)
        {
            InnerConnection = new TcpConnection(useSsl);
            await InnerConnection.ConnectAsync(host, cancellationToken);
            return InnerConnection;
        }

        public override void Dispose()
        {
            InnerConnection.Dispose();
            base.Dispose();
        }
    }
}