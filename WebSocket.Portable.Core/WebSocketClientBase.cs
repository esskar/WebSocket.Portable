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
        private int _maxFrameDataLength = Consts.MaxDefaultFrameDataLength;
        
        public event Action Opened;
        public event Action Closed;
        public event Action<Exception> Error;
        public event Action<IWebSocketFrame> FrameReceived;
        public event Action<IWebSocketMessage> MessageReceived;

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

        /// <summary>
        /// Gets or sets the maximum length of data to be send in a single frame. If data is to be send is bigger, data will be fragmented.
        /// </summary>
        /// <value>
        /// The maximum length of the frame data.
        /// </value>
        /// <exception cref="System.ArgumentOutOfRangeException">value</exception>
        public int MaxFrameDataLength
        {
            get { return _maxFrameDataLength; }
            set
            {
                if (_maxFrameDataLength == value) 
                    return;

                if (value <= 0 || value > Consts.MaxAllowedFrameDataLength)
                    throw new ArgumentOutOfRangeException("value", string.Format("Value must be between 1 and {0}", Consts.MaxAllowedFrameDataLength));
                _maxFrameDataLength = value;
            }
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
            var task = TaskAsyncHelper.Empty;
            var max = this.MaxFrameDataLength;
            var opcode = isBinary ? WebSocketOpcode.Binary : WebSocketOpcode.Text;
            while (length > 0)
            {
                var size = Math.Min(length, max);
                length -= size;

                var frame = new WebSocketClientFrame
                {
                    Opcode = opcode,
                    IsFin = length == 0,
                };
                frame.Payload = new WebSocketPayload(frame, bytes, offset, size);
                offset += size;

                task = task.Then(f => this.SendAsync(f, cancellationToken), frame);
            }
            return task;            
        }

        private Task SendAsync(IWebSocketFrame frame, CancellationToken cancellationToken)
        {
            return _webSocket.SendFrameAsync(frame, cancellationToken);
        }

        protected virtual void OnError(Exception exception)
        {
            var handler = this.Error;
            if (handler != null)
                handler(exception);
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

        protected virtual void OnMessageReceived(IWebSocketMessage message)
        {
            var handler = this.MessageReceived;
            if (handler != null)
                handler(message);
        }

        private async void ReceiveLoop()
        {
            _cts = new CancellationTokenSource();

            WebSocketMessage currentMessage = null;
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
                    else if (frame.IsDataFrame)
                    {
                        if (currentMessage != null)
                            throw new WebSocketException(WebSocketErrorCode.CloseInconstistentData);
                        currentMessage = new WebSocketMessage();
                        currentMessage.AddFrame(frame);
                    }
                    else if (frame.Opcode == WebSocketOpcode.Continuation)
                    {
                        if (currentMessage == null)
                            throw new WebSocketException(WebSocketErrorCode.CloseInconstistentData);
                        currentMessage.AddFrame(frame);                        
                    }
                    else
                    {                       
                        this.LogDebug("Other frame received: {0}", frame.Opcode);
                    }

                    if (currentMessage != null && currentMessage.IsComplete)
                    {
                        this.OnMessageReceived(currentMessage);
                        currentMessage = null;
                    }
                }
                catch (WebSocketException wsex)
                {
                    this.LogError("An web socket error occurred.", wsex);
                    this.OnError(wsex);
                    break;
                }
                catch (Exception ex)
                {
                    this.LogError("An unexpected error occurred.", ex);
                    this.OnError(ex);
                    break;
                }
            }
        }
    }
}
