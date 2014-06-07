## WebSocket.Portable

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
	
	// get notified when data has arrived
	client.FrameReceived += frame => Console.WriteLine(frame);

	// open a web socket connection to ws://echo.websocket.org
    await client.OpenAsync("ws://echo.websocket.org");

	// send some data
    await client.SendAsync("WebSocket.Portable rocks!");

### Questions

Ask questions in the [WebSocket.Portable][2] room on [JabbR][3].

[1]: http://www.rfc-editor.org/rfc/rfc6455.txt
[2]: https://jabbr.net/#/rooms/WebSocketPortable
[3]: https://jabbr.net/
