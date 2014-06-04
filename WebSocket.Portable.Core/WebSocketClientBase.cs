using System;
using System.Threading;
using System.Threading.Tasks;
using WebSocket.Portable.Interfaces;
using WebSocket.Portable.Tasks;

namespace WebSocket.Portable
{
    public abstract class WebSocketClientBase<TWebSocket> : IDisposable 
        where TWebSocket : class, IWebSocket, new()
    {
        private TWebSocket _webSocket;

        ~WebSocketClientBase()
        {
            this.Dispose(false);
        }

        public Task OpenAsync(string uri)
        {
            return this.OpenAsync(uri, CancellationToken.None);
        }

        public async Task OpenAsync(string uri, CancellationToken cancellationToken)
        {
            if (_webSocket != null)
                throw new InvalidOperationException("Client has been opened before.");

            _webSocket = new TWebSocket();
            await _webSocket.ConnectAsync(uri, cancellationToken);
            await _webSocket.SendHandshakeAsync(cancellationToken);
        }

        public Task CloseAsync()
        {
            return this.CloseAsync(CancellationToken.None);
        }

        public Task CloseAsync(CancellationToken cancellationToken)
        {
            return _webSocket == null
                ? TaskAsyncHelper.Empty
                : _webSocket.CloseAsync(WebSocketErrorCode.CloseNormal);
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
                _webSocket.Dispose();
        }
    }
}
