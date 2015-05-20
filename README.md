## WebSocket.Portable

WebSocket.Portable is a PCL C# implementation of the [WebSocket protocol](https://tools.ietf.org/html/rfc6455). This plugin uses .net 4.5 and is compatible with Windows 8, Windows Phone, Windows Phone Silverlight, Xamarin Android, Xamarin iOS, and Xamarin iOS (Classic). This is a fork from [esskar](https://github.com/esskar/WebSocket.Portable).

### NuGet
https://www.nuget.org/packages/WebSocket.Portable.Core/

### Modifications

- Upgraded PCL assembly targets
- Added sockets-for-pcl websocket implementation (was using native implementations before).
 
### Dependencies

- Microsoft Bcl (NuGet)
  - Microsoft.Bcl
  - Microsoft.Bcl.Async
  - Microsoft.Bcl.Build
  - Microsoft.Net.Http
- [Sockets For PCL](https://github.com/rdavisau/sockets-for-pcl) (NuGet)

### TODO

- Remove abstraction which is no longer needed.
- Xamarin.Forms sample


### Thanks

- rdavisau : For sockets-for-pcl which makes this possible. A great developer to work with who jumped to the rescue.

- esskar : For started the websocket abstraction.

- Xamarin : For awesome cross platform development tools


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
