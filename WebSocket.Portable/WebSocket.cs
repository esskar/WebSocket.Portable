using System.Threading;
using System.Threading.Tasks;
using WebSocket.Portable.Interfaces;
using WebSocket.Portable.Net;

namespace WebSocket.Portable
{
    public class WebSocket : WebSocketBase    
    {
        protected override async Task<ITcpConnection> ConnectAsync(string host, int port, bool useSsl, CancellationToken cancellationToken)
        {
            var tcp = new TcpConnection(useSsl);
            await tcp.ConnectAsync(host, port, cancellationToken);
            return tcp;
        }
    }
}