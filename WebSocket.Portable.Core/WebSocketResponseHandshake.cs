using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using WebSocket.Portable.Resources;

namespace WebSocket.Portable
{
    public class WebSocketResponseHandshake : HttpResponseMessage
    {
        public static WebSocketResponseHandshake Parse(IList<string> responseLines)
        {            
            if (responseLines == null || responseLines.Count < 1)
                throw new ArgumentException(ErrorMessages.NoHeaderLines, "responseLines");

            var responseLine = responseLines[0].Split(' ');
            if (responseLine.Length < 3)
                throw new ArgumentException(ErrorMessages.InvalidResponseLine + responseLines[0], "responseLines");

            var response = new WebSocketResponseHandshake
            {
                StatusCode = (HttpStatusCode)Convert.ToInt32(responseLine[1]),
                ReasonPhrase = string.Join(" ", responseLine.Skip(2)),
                Version = new Version(responseLine[0].Substring(5)), // "HTTP/x.x"            
            };

            foreach (var line in responseLines.Skip(1))
            {
                if (string.IsNullOrEmpty(line))
                    break;

                var pos = line.IndexOf(':');
                if (pos < 0)
                    continue;

                var key = line.Substring(0, pos).Trim();
                var value = line.Substring(pos + 1).Trim();

                if (key.Equals("date", StringComparison.OrdinalIgnoreCase))
                {
                    response.Headers.Add(key, value);
                }
                else
                {
                    try
                    {
                        response.Headers.Add(key, value.Split(',').Select(v => v.Trim()).Where(v => v.Length > 0));
                    }
                    catch
                    {
                        response.Headers.Add(key, value);
                    }                                                    
                }                
            }
            

            return response;
        }

        public string SecWebSocketAccept
        {
            get { return this.GetHeader("Sec-WebSocket-Accept"); }
            set { this.SetHeader("Sec-WebSocket-Accept", value); }
        }

        public IList<string> SecWebSocketProtocol
        {
            get { return this.GetHeaders("Sec-WebSocket-Protocol"); }
            set { this.SetHeaders("Sec-WebSocket-Protocol", value); }
        }

        public IList<string> SecWebSocketVersion
        {
            get { return this.GetHeaders("Sec-WebSocket-Version"); }
            set { this.SetHeaders("Sec-WebSocket-Version", value); }
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
    }
}
