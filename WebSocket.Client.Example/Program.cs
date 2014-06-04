using System;
using WebSocket.Portable;
using WebSocket.Portable.Interfaces;
using WebSocket.Portable.Internal;

namespace WebSocket.Client.Example
{
    class Program
    {
        static void Main()
        {
            var client = new WebSocketClient();
            client.OpenAsync("ws://echo.websocket.org").Wait();

            Console.WriteLine("Client connected, enter text and send it with pressing <ENTER>");
            var text = Console.ReadLine();
            while (!string.IsNullOrEmpty(text))
            {
                client.SendAsync(text);
                text = Console.ReadLine();
            }
        }        
    }
}
