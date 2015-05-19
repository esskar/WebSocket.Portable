using System;

namespace WebSocket.Portable
{
    public class WebSocketClient : WebSocketClientBase<WebSocket>
    {
        protected override void OnError(Exception exception)
        {
            if(exception is ObjectDisposedException){
                var e =(ObjectDisposedException) exception;
                System.Diagnostics.Debug.WriteLine(e.ObjectName);
            }
            base.OnError(exception);
        }
    }
}
