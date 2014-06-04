using System.Text;
using WebSocket.Portable.Interfaces;

namespace WebSocket.Portable
{
    internal class WebSocketPayload : IWebSocketPayload
    {
        private readonly byte[] _data;
        private readonly int _offset;
        private readonly int _length;

        public WebSocketPayload(byte[] data = null)
        {
            _data = data;
            if (_data != null)
                _length = _data.Length;
        }

        public WebSocketPayload(string data)
            : this(Encoding.UTF8.GetBytes(data ?? string.Empty)) { }

        public WebSocketPayload(byte[] data, int offset, int length)
        {
            _data = data;
            _offset = offset;
            _length = length;
        }

        public byte[] Data
        {
            get { return _data; }
        }

        public int Offset
        {
            get { return _offset; }
        }

        public int Length
        {
            get { return _length; }
        }
    }
}
