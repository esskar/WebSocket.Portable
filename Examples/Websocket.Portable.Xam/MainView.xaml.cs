using System;
using System.Collections.ObjectModel;
using WebSocket.Portable;
using WebSocket.Portable.Interfaces;
using Xamarin.Forms;

namespace Websocket.Portable.Xam
{
	public class SocketMessage{
		public string Content {get;set;}
	}

	public partial class MainView : ContentPage
	{
		private string _serverUrl = "ws://echo.websocket.org";
		public string ServerUrl {
			get {
				return _serverUrl;
			}
			set {
				if (_serverUrl == value)
					return;
				_serverUrl = value;
				OnPropertyChanged ();
			}
		}

		private string _message = "Hello World";
		public string Message {
			get {
				return _message;
			}
			set {
				if (_message == value)
					return;
				_message = value;
				OnPropertyChanged ();
			}
		}


		private ObservableCollection<SocketMessage> _output = new ObservableCollection<SocketMessage>();
		public ObservableCollection<SocketMessage> Output {
			get {
				return _output;
			}
			set {
				if (_output == value)
					return;
				_output = value;
				OnPropertyChanged ();
			}
		}

		WebSocketClient Client;

		public MainView ()
		{
			BindingContext = this;
			InitializeComponent ();
			Init ();
		}

		async void Init(){		

			Client = new WebSocketClient ();
			Client.MessageReceived += OnMessage;

			await Client.OpenAsync("ws://echo.websocket.org");

			OnMessage("Client connected.");

		}

		private void OnMessage(IWebSocketMessage obj)
		{
			Output.Add(new SocketMessage{ Content = obj.ToString()});
		}
		private void OnMessage(string obj)
		{
			Output.Add(new SocketMessage{ Content = obj});
		}

		public void DoSend(object sender, EventArgs e){

			Client.SendAsync(Message);
		}
	}
}

