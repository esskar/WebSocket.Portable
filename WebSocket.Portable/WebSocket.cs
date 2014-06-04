using System.Threading;
using System.Threading.Tasks;
using WebSocket.Portable.Interfaces;
using WebSocket.Portable.Net;

namespace WebSocket.Portable
{
    public class WebSocket : WebSocketBase    
    {
        protected override async Task<ITcpConnection> ConnectAsync(string host, int port, CancellationToken cancellationToken)
        {
            var tcp = new TcpConnection();
            await tcp.ConnectAsync(host, port, cancellationToken);
            return tcp;
        }
    }
}