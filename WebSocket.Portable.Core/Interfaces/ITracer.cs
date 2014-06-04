using System;
using WebSocket.Portable.Internal;

namespace WebSocket.Portable.Interfaces
{
    public interface ITracer
    {
        void Trace(Type type, LogLevel logLevel, string message);
    }
}
