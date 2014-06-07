using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebSocket.Portable.Interfaces;
using WebSocket.Portable.Internal;
using WebSocket.Portable.Tasks;

namespace WebSocket.Portable
{
    public abstract class WebSocketClientBase<TWebSocket> : IDisposable, ICanLog
        where TWebSocket : class, IWebSocket, new()
    {
        private TWebSocket _webSocket;
        private CancellationTokenSource _cts;

        public event Action Opened;
        public event Action Closed;
        public event Action<Exception> Error;
        public event Action<IWebSocketFrame> FrameReceived;

        protected WebSocketClientBase()
        {
            this.AutoSendPongResponse = true;
        }

        ~WebSocketClientBase()
        {
            this.Dispose(false);
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

        /// <summary>
        /// Gets or sets a value indicating whether to send automatically pong frames when a ping is received.
        /// </summary>
        /// <value>
        /// <c>true</c> if pong frames are send automatically; otherwise, <c>false</c>.
        /// </value>
        public bool AutoSendPongResponse { get; set; }                       

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

            this.OnOpened();

            this.ReceiveLoop();
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

        public Task SendAsync(string text)
        {
            return this.SendAsync(text, CancellationToken.None);
        }

        public Task SendAsync(string text, CancellationToken cancellationToken)
        {
            if (text == null)
                throw new ArgumentNullException("text");

            var bytes = Encoding.UTF8.GetBytes(text);
            return this.SendAsync(false, bytes, 0, bytes.Length, cancellationToken);
        }

        public Task SendAsync(byte[] bytes, int offset, int length)
        {
            return this.SendAsync(bytes, offset, length, CancellationToken.None);
        }

        public Task SendAsync(byte[] bytes, int offset, int length, CancellationToken cancellationToken)
        {
            return this.SendAsync(true, bytes, offset, length, cancellationToken);
        }

        private Task SendAsync(bool isBinary, byte[] bytes, int offset, int length, CancellationToken cancellationToken)
        {
            var frame = new WebSocketClientFrame
            {
                Opcode = isBinary ? WebSocketOpcode.Binary : WebSocketOpcode.Text,                
            };
            frame.Payload = new WebSocketPayload(frame, bytes, offset, length);
            return this.SendAsync(frame, cancellationToken);
        }

        private Task SendAsync(IWebSocketFrame frame, CancellationToken cancellationToken)
        {
            return _webSocket.SendFrameAsync(frame, cancellationToken);
        }

        protected virtual void OnOpened()
        {
            var handler = this.Opened;
            if (handler != null)
                handler();
        }

        protected virtual void OnFrameReceived(IWebSocketFrame frame)
        {
            var handler = this.FrameReceived;
            if (handler != null)
                handler(frame);
        }

        private async void ReceiveLoop()
        {
            _cts = new CancellationTokenSource();
            while (!_cts.IsCancellationRequested)
            {
                try
                {
                    var frame = await _webSocket.ReceiveFrameAsync(_cts.Token);
                    if (frame == null)
                    {
                        // todo
                        break;
                    }

                    this.OnFrameReceived(frame);

                    if (frame.Opcode == WebSocketOpcode.Close)
                    {
                        break;
                    }
                    if (frame.IsControlFrame)
                    {
                        // Handle ping frame
                        if (frame.Opcode == WebSocketOpcode.Ping && this.AutoSendPongResponse)
                        {
                            var pongFrame = new WebSocketClientFrame
                            {
                                Opcode = WebSocketOpcode.Pong,
                                Payload = frame.Payload
                            };
                            await this.SendAsync(pongFrame, _cts.Token);
                        }
                    }
                    else
                    {
                        this.LogDebug("Other frame received: {0}", frame.Opcode);
                    }
                }
                catch (WebSocketException wsex)
                {
                    this.LogError("An web socket error occurred.", wsex);

                    // todo
                    break;
                }
                catch (Exception ex)
                {
                    this.LogError("An unexpected error occurred.", ex);

                    // todo
                    break;
                }
            }
        }
    }
}
