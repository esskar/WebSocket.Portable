using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using WebSocket.Portable.Internal;
using WebSocket.Portable.Resources;

namespace WebSocket.Portable
{
    public class WebSocketRequestHandshake : HttpRequestMessage
    {
        private WebSocketRequestHandshake(HttpMethod method)
        {
            this.Upgrade = "websocket";
            this.Connection = "Upgrade";
            this.SecWebSocketProtocol = new[] {"chat", "superchat"};
            this.SecWebSocketVersion = Consts.SupportedClientVersions[0];
            this.SecWebSocketKey = WebSocketHelper.CreateClientKey();

            this.Method = method;
        }

        public WebSocketRequestHandshake(Uri uri)
            : this(uri, uri) { }

        public WebSocketRequestHandshake(HttpMethod method, Uri uri)
            : this(method, uri, uri) { }

        public WebSocketRequestHandshake(Uri uri, Uri originUri)
            : this(HttpMethod.Get, uri, originUri) { }

        public WebSocketRequestHandshake(HttpMethod method, Uri uri, Uri originUri)
            : this(method)
        {
            if (uri == null)
                throw new ArgumentNullException("uri");
            if (!uri.IsAbsoluteUri)
                throw new ArgumentException(ErrorMessages.NotAnAbsoluteUri, "uri");
            if (originUri == null)
                throw new ArgumentNullException("originUri");
            if (!originUri.IsAbsoluteUri)
                throw new ArgumentException(ErrorMessages.NotAnAbsoluteUri, "originUri");   
            
            // uri
            this.RequestUri = new Uri(uri.AbsolutePath + uri.Query, UriKind.Relative);
            this.Host = uri.Host;

            // orgin
            var scheme = originUri.Scheme == "http" || originUri.Scheme == "ws"
                ? "http" : originUri.Scheme == "https" || originUri.Scheme == "wss" 
                ? "https" : null;
            if (scheme == null)
                throw new ArgumentException(ErrorMessages.InvalidScheme + originUri.Scheme, "originUri");

            var origin = new StringBuilder();
            origin.AppendFormat("{0}://{1}", scheme, originUri.Host);
            if (scheme == "http" && originUri.Port != 80 || scheme == "https" && originUri.Port != 443)
                origin.AppendFormat("{0}", originUri.Port);
            origin.Append(originUri.AbsolutePath);
            
            this.Origin = origin.ToString();            
        }

        public string Connection
        {
            get
            {
                return this.Headers.Connection.FirstOrDefault();
            }
            set
            {
                this.Headers.Connection.Clear();
                if (!string.IsNullOrEmpty(value))
                    this.Headers.Connection.Add(value);
            }
        }

        public string Host
        {
            get { return this.Headers.Host; }
            set { this.Headers.Host = value; }
        }

        public string Origin
        {
            get { return this.GetHeader("Origin"); }
            set { this.SetHeader("Origin", value); }
        }

        public IList<string> SecWebSocketExtensions
        {
            get { return this.GetHeaders("Sec-WebSocket-Extensions"); }
            set { this.SetHeaders("Sec-WebSocket-Extensions", value); }
        }

        public string SecWebSocketKey
        {
            get { return this.GetHeader("Sec-WebSocket-Key"); }
            set { this.SetHeader("Sec-WebSocket-Key", value); }
        }

        public IList<string> SecWebSocketProtocol
        {
            get { return this.GetHeaders("Sec-WebSocket-Protocol"); }
            set { this.SetHeaders("Sec-WebSocket-Protocol", value); }
        }

        public string SecWebSocketVersion
        {
            get { return this.GetHeader("Sec-WebSocket-Version"); }
            set { this.SetHeader("Sec-WebSocket-Version", value); }
        }

        public string Upgrade
        {
            get
            {
                var upgrade = this.Headers.Upgrade.FirstOrDefault();
                return upgrade != null ? upgrade.Name : null;
            }
            set
            {
                this.Headers.Upgrade.Clear();
                if (!string.IsNullOrEmpty(value))
                    this.Headers.Upgrade.Add(new ProductHeaderValue(value));
            }
        }        

        private string GetHeader(string key)
        {
            IEnumerable<string> values;
            return !this.Headers.TryGetValues(key, out values) ? null : values.FirstOrDefault();
        }

        private IList<string> GetHeaders(string key)
        {
            IEnumerable<string> values;
            return !this.Headers.TryGetValues(key, out values) ? null : values.ToList();
        }

        private void SetHeader(string key, string value)
        {
            this.Headers.Remove(key);
            if (!string.IsNullOrEmpty(value))
                this.Headers.Add(key, value);
        }

        private void SetHeaders(string key, IEnumerable<string> values)
        {
            this.Headers.Remove(key);
            if (values == null) 
                return;

            foreach (var value in values)
                this.Headers.Add(key, value);
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            // TODO: Proxy support
            sb.AppendFormat("{0} {1} HTTP/{2}\r\n",
                this.Method.Method, this.RequestUri, this.Version);
            foreach (var header in this.Headers)
            {
                sb.AppendFormat("{0}: ", header.Key);
                var valueCount = 0;
                foreach (var value in header.Value)
                {
                    if (valueCount > 0)
                        sb.Append(", ");
                    sb.Append(value);
                    ++valueCount;
                }
                sb.Append("\r\n");
            }
            sb.Append("\r\n");

            return sb.ToString();
        }
    }
}