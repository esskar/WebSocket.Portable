using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using WebSocket.Portable;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace WebSocket.Universal
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();

            Tests();
        }


        private async Task Tests()
        {
            // socket Test
            await TestRda();

            // Websocket Test
            await TestWebsocketPortable();
        }


        private async Task TestRda()
        {
            var socket = new Sockets.Plugin.TcpSocketClient();

            //Never Ending
            await socket.ConnectAsync("echo.websocket.org", 80, false);

            var b = socket.ReadStream.ReadByte();

            Debug.WriteLine("TestRda");
        }

        private async Task TestWebsocketPortable()
        {
            var client = new WebSocketClient();
            client.Opened += websocket_Opened;
            client.Closed += websocket_Closed;
            client.MessageReceived += websocket_MessageReceived;

            //Never Ending
            await client.OpenAsync("ws://echo.websocket.org");

            await client.SendAsync("Hello World");

            await client.SendAsync("Hello World2");

            Debug.WriteLine("TestWebsocketPortable");
        }

        void websocket_MessageReceived(Portable.Interfaces.IWebSocketMessage obj)
        {
            Debug.WriteLine(obj.ToString());
        }

        void websocket_Closed()
        {
            Debug.WriteLine("websocket_Opened");
        }

        void websocket_Opened()
        {
            Debug.WriteLine("websocket_Opened");
        }
    }
}
