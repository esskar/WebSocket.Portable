﻿using System;
using WebSocket.Portable;
using WebSocket.Portable.Interfaces;

namespace WebSocket.Client.Example
{
    class Program
    {
        static void Main()
        {
            var client = new WebSocketClient();
            client.MessageReceived += m =>
            {
                if (m.IsText)
                    Console.WriteLine("RESPONSE: {0}", m.ToString());
            };
            client.OpenAsync("wss://echo.websocket.org").Wait();

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
