using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebSocket.Portable.Interfaces;
using WebSocket.Portable.Internal;
using WebSocket.Portable.Resources;
using WebSocket.Portable.Security;
using WebSocket.Portable.Tasks;

namespace WebSocket.Portable
{
    public abstract class WebSocketBase : IWebSocket, ICanLog
    {
        private readonly AsyncLock _asyncLock;
        private readonly List<IWebSocketExtension> _extensions;
        private Uri _uri;
        private WebSocketState _state;
        private ITcpConnection _tcp;

        /// <summary>
        /// Prevents a default instance of the <see cref="WebSocketBase"/> class from being created.
        /// </summary>
        protected WebSocketBase()
        {
            _asyncLock = new AsyncLock();
            _extensions = new List<IWebSocketExtension>();
            _state = WebSocketState.Closed;
        }

        public async void RegisterExtension(IWebSocketExtension extension)
        {
            if (extension == null)
                throw new ArgumentNullException("extension");

            if (_extensions.Contains(extension))
                throw new ArgumentException(ErrorMessages.ExtensionsAlreadyRegistered + extension.Name, "extension");

            using (await _asyncLock.LockAsync())
            {
                if (_state != WebSocketState.Closed)
                    throw new InvalidOperationException(ErrorMessages.InvalidState + _state);
                _extensions.Add(extension);
            }
        }

        public Task CloseAsync(WebSocketErrorCode errorCode)
        {
            return this.CloseAsync(errorCode, CancellationToken.None);
        }

        public Task CloseAsync(WebSocketErrorCode errorCode, CancellationToken cancellationToken)
        {
            return this.RunAsync(WebSocketState.Open, WebSocketState.Closing, WebSocketState.Closed,
                async () =>
                      {
                          await TaskAsyncHelper.Empty;
                      }, cancellationToken);
        }

        /// <summary>
        /// Connects asynchronous.
        /// </summary>
        /// <param name="uri">The URI.</param>
        /// <returns></returns>
        public Task ConnectAsync(string uri)
        {
            return this.ConnectAsync(uri, CancellationToken.None);
        }

        /// <summary>
        /// Connects asynchronous.
        /// </summary>
        /// <param name="uri">The URI.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="System.InvalidOperationException">Cannot connect because current state is  + _state</exception>
        public Task ConnectAsync(string uri, CancellationToken cancellationToken)
        {
            return this.RunAsync(WebSocketState.Closed, WebSocketState.Connecting, WebSocketState.Connected,
                async () =>
                {
                    if (uri == null)
                        throw new ArgumentNullException("uri");

                    _uri = WebSocketHelper.CreateWebSocketUri(uri);

                    var useSsl = _uri.Scheme == Consts.SchemeWss;
                    _tcp = await this.ConnectAsync(_uri.DnsSafeHost, _uri.Port, useSsl, cancellationToken);
                }, cancellationToken);
        }


        /// <summary>
        /// Connects asynchronous.
        /// </summary>
        /// <param name="host">The host.</param>
        /// <param name="port">The port.</param>
        /// <param name="useSsl">if set to <c>true</c> [use SSL].</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        protected abstract Task<ITcpConnection> ConnectAsync(string host, int port, bool useSsl, CancellationToken cancellationToken);

        /// <summary>
        /// Sends the default handshake asynchronous.
        /// </summary>
        /// <returns></returns>
        public Task<WebSocketResponseHandshake> SendHandshakeAsync()
        {
            return this.SendHandshakeAsync(CancellationToken.None);
        }

        /// <summary>
        /// Sends the default handshake asynchronous.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public Task<WebSocketResponseHandshake> SendHandshakeAsync(CancellationToken cancellationToken)
        {
            var handshake = new WebSocketRequestHandshake(_uri);
            foreach (var extension in _extensions)
                handshake.AddExtension(extension);

            return this.SendHandshakeAsync(handshake, cancellationToken);
        }

        /// <summary>
        /// Sends the handshake asynchronous.
        /// </summary>
        /// <param name="handshake">The handshake.</param>
        /// <returns></returns>
        public Task<WebSocketResponseHandshake> SendHandshakeAsync(WebSocketRequestHandshake handshake)
        {
            return this.SendHandshakeAsync(handshake, CancellationToken.None);
        }

        /// <summary>
        /// Sends the handshake asynchronous.
        /// </summary>
        /// <param name="handshake">The handshake.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public Task<WebSocketResponseHandshake> SendHandshakeAsync(WebSocketRequestHandshake handshake, CancellationToken cancellationToken)
        {
            return this.RunAsync(WebSocketState.Connected, WebSocketState.Opening, WebSocketState.Open,
                async () =>
                {
                    var data = handshake.ToString();
                    await this.SendAsync(data, Encoding.UTF8, cancellationToken);

                    var responseHeaders = new List<string>();
                    var line = await _tcp.ReadLineAsync(cancellationToken);
                    while (!String.IsNullOrEmpty(line))
                    {
                        responseHeaders.Add(line);
                        line = await _tcp.ReadLineAsync(cancellationToken);
                    }

                    var response = WebSocketResponseHandshake.Parse(responseHeaders);
                    if (response.StatusCode != HttpStatusCode.SwitchingProtocols)
                    {
                        var versions = response.SecWebSocketVersion;
                        if (versions != null && !versions.Intersect(Consts.SupportedClientVersions).Any())
                            throw new WebSocketException(WebSocketErrorCode.HandshakeVersionNotSupported);

                        throw new WebSocketException(WebSocketErrorCode.HandshakeInvalidStatusCode);
                    }

                    var challenge = Encoding.UTF8.GetBytes(handshake.SecWebSocketKey + Consts.ServerGuid);
                    var hash = Sha1Digest.ComputeHash(challenge);
                    var calculatedAccept = Convert.ToBase64String(hash);

                    if (response.SecWebSocketAccept != calculatedAccept)
                        throw new WebSocketException(WebSocketErrorCode.HandshakeInvalidSecWebSocketAccept);

                    response.RequestMessage = handshake;

                    return response;

                }, cancellationToken);
        }

        public Task SendFrameAsync(IWebSocketFrame frame)
        {
            return this.SendFrameAsync(frame, CancellationToken.None);
        }

        public Task SendFrameAsync(IWebSocketFrame frame, CancellationToken cancellationToken)
        {
            if (frame == null)
                throw new ArgumentNullException("frame");

            return frame.WriteToAsync(_tcp, cancellationToken);
        }

        public Task<IWebSocketFrame> ReceiveFrameAsync()
        {
            return this.ReceiveFrameAsync(CancellationToken.None);
        }

        public Task<IWebSocketFrame> ReceiveFrameAsync(CancellationToken cancellationToken)
        {
            var frame = new WebSocketServerFrame();
            return frame.ReadFromAsync(_tcp, cancellationToken).Then(() => (IWebSocketFrame)frame);
        }

        /// <summary>
        /// Sends data asynchronous.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="encoding">The encoding.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        private Task SendAsync(string data, Encoding encoding, CancellationToken cancellationToken)
        {
            var bytes = encoding.GetBytes(data);
            return this.SendAsync(bytes, 0, bytes.Length, cancellationToken);
        }

        /// <summary>
        /// Sends data asynchronous.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="length">The length.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        private Task SendAsync(byte[] buffer, int offset, int length, CancellationToken cancellationToken)
        {
            return _tcp.WriteAsync(buffer, offset, length, cancellationToken);
        }

        public void Dispose()
        {
            // TODO
        }

        private Task RunAsync(WebSocketState requiredState, WebSocketState intermediateState,
            WebSocketState finalState, Func<Task> action, CancellationToken cancellationToken)
        {
            if (action == null)
                throw new ArgumentNullException("action");

            return this.SetStateIfAsync(requiredState, intermediateState, cancellationToken)
                .Then(action)
                .Then(() => this.SetStateIfAsync(intermediateState, finalState, cancellationToken));
        }

        private async Task<T> RunAsync<T>(WebSocketState requiredState, WebSocketState intermediateState,
            WebSocketState finalState, Func<Task<T>> action, CancellationToken cancellationToken)
        {
            if (action == null)
                throw new ArgumentNullException("action");

            // can this be done without using async/await?

            await this.SetStateIfAsync(requiredState, intermediateState, cancellationToken);

            var retval = await action();

            await this.SetStateIfAsync(intermediateState, finalState, cancellationToken);

            return retval;
        }

        private Task SetStateIfAsync(WebSocketState requiredState, WebSocketState newState, CancellationToken cancellationToken)
        {
            return _asyncLock.LockAsync(cancellationToken).Then(() =>
            {
                if (_state != requiredState)
                    throw new InvalidOperationException(ErrorMessages.InvalidState + _state);
                _state = newState;
            });
        }
    }
}
