using System;
using System.Collections.Generic;
using System.Text;
using Sockets.Plugin;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;
using WebSocket.Portable;

namespace Websocket.Portable.Xam
{
	public class Tester{

		System.Diagnostics.Stopwatch watch;
		WebSocket.Portable.WebSocketClient client;
		const string connectionUrl = "ws://";

		Action OnOpen;

		public Tester (Action onOpen)
		{
			OnOpen = onOpen;
			client = new WebSocket.Portable.WebSocketClient ();
			watch = new System.Diagnostics.Stopwatch ();
			client.FrameReceived += OnFrame;
		}

		public async void Connect ()
		{
			watch.Start ();
			await client.OpenAsync (connectionUrl);
		}	

		async void Done(){
			client.FrameReceived -= OnFrame;
			await client.CloseAsync ();
			client.Dispose ();
			OnOpen ();
		}

		void OnFrame(WebSocket.Portable.Interfaces.IWebSocketFrame frame){
			watch.Stop ();
			System.Diagnostics.Debug.WriteLine(frame.ToString());
			//System.Diagnostics.Debug.WriteLine(watch.ElapsedMilliseconds);
			Done ();
		}
	}


	public partial class TestView : ContentPage
	{
		// Counting Opens vs Tests
		// If they dont match we are missing o's
		int Opens;
		int Tests;

		public TestView ()
		{
			InitializeComponent ();
		}

		public void OnButtonClicked (object sender, EventArgs args)
		{
			//System.Diagnostics.Debug.WriteLine(String.Format("{0}/{1} {2}/{3}",Tests, Opens, WebSocketFrame.Reads, WebSocketFrame.Reads2));
			System.Diagnostics.Debug.WriteLine(String.Format("{0}/{1}",Tests, Opens));
			Tests++;
			var tester = new Tester (()=>{Opens++;});
			tester.Connect ();
		}
	}
}