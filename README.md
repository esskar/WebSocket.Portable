# WebSocket.Portable

WebSocket.Portable is a portable C# implementation of 
the [WebSocket][1] protocol.

### Status

The WebSocket.Portable library is still in pre-alpha state, 
still lots of features are missing, still lots of tests to write, 
interfaces will probably change.

### Installation

WebSocket.Portable will be available via NuGet.

### Usage

    var client = new WebSocketClient();
	client.DataReceived += d => Console.WriteLine(d.GetText());
    await client.OpenAsync("ws://echo.websocket.org");
    await client.SendAsync("WebSocket.Portable rocks!");

[1]: http://www.rfc-editor.org/rfc/rfc6455.txt
