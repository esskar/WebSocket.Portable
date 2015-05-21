using System;

namespace WebSocket.Portable
{
    public class WebSocketClient : WebSocketClientBase<WebSocket>
    {
        protected override void OnError(Exception exception)
        {
            if(exception is ObjectDisposedException){
                var e =(ObjectDisposedException) exception;
            }
            base.OnError(exception);
        }
    }
}
