using System;
using System.Threading.Tasks;
using WebSocket.Portable;
using WebSocket.Portable.Interfaces;
using WebSocket.Portable.Internal;

namespace WebSocket.Client.Example
{
    class Program
    {
        static void Main(string[] args)
        {
            var client = new WebSocketClient {Tracer = new ConsoleTracer()};
            client.OpenAsync("ws://echo.websocket.org").Wait();

            Console.WriteLine("Client connected, enter text and send it with pressing <ENTER>");
            var text = Console.ReadLine();
            while (!string.IsNullOrEmpty(text))
            {
                client.SendAsync(text);
                text = Console.ReadLine();
            }
        }

        class ConsoleTracer : ITracer
        {
            public void Trace(Type type, LogLevel logLevel, string message)
            {
                Console.WriteLine("{0} - {1}: {2}", logLevel, type.Name, message);
            }
        }
    }
}
