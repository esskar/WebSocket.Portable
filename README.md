## WebSocket.Portable

WebSocket.Portable is a PCL C# implementation of the [WebSocket](https://tools.ietf.org/html/rfc6455) protocol. This plugin uses .net 4.5 and is compatible with Windows 8, Windows Phone, Windows Phone Silverlight, Xamarin Android, Xamarin iOS, and Xamarin iOS (Classic). This is a fork from [esskar](https://github.com/esskar/WebSocket.Portable).

### Modifications

- Upgraded PCL assembly targets
- Included [sockets-for-pcl](https://github.com/rdavisau/sockets-for-pcl) socket implementation.
- Tied everything together
- 
### Dependencies

- Microsoft HTTP Client Libraries (NuGet)

### TODO

- Merge with esskar ot Publish onto  NuGet.
- Remove abstraction which is no longer needed.

### Usage

    var client = new WebSocketClient();
	
	// get notified when data has arrived
	client.FrameReceived += frame => Console.WriteLine(frame);

	// open a web socket connection to ws://echo.websocket.org
    await client.OpenAsync("ws://echo.websocket.org");

	// send some data
    await client.SendAsync("WebSocket.Portable rocks!");

### Questions

Post onto the Github [issue system](https://github.com/NVentimiglia/WebSocket.Portable) or contact me via my [blog](http://nicholasventimiglia.com)
