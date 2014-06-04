using System.Text;

namespace WebSocket.Portable.Interfaces
{
    public interface IWebSocketPayload
    {
        byte[] Data { get; }

        int Offset { get; }

        int Length { get; }
    }

    public static class WebSocketPayloadExtensions
    {
        public static string GetText(this IWebSocketPayload payload)
        {
            if (payload == null || payload.Data == null)
                return string.Empty;

            return Encoding.UTF8.GetString(payload.Data, payload.Offset, payload.Length);
        }
    }
}