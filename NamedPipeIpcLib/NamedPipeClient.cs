using System;
using System.Diagnostics;
using System.IO.Pipes;

namespace NamedPipeIpcLib
{
	public class NamedPipeClient : NamedPipeEndpointBase<NamedPipeClientStream>
	{
		private const string LocalComputer = ".";

		public NamedPipeClient(string pipeName)
			: base(pipeName)
		{
		}

		protected override bool AutoFlushPipeWriter
		{
			get { return true; }
		}

		protected override NamedPipeClientStream CreateStream()
		{
			var stream = new NamedPipeClientStream(
				serverName: LocalComputer, 
				pipeName: PipeName, 
				direction: PipeDirection.InOut, 
				options: PipeOptions.Asynchronous)
			{
				ReadMode = PipeTransmissionMode.Message
			};
			stream.Connect();

			return stream;
		}

		protected override void ReadFromPipe()
		{
			try
			{
				while (Pipe != null && !StopRequested)
				{
					if (Pipe.IsConnected)
					{
						string message = ReadMessage();
						OnMessageReceived(new MessageReceivedEventArgs(message));
					}
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.ToString());
			}
			finally
			{
				Debug.WriteLine(@"Client shutting down.");
			}
		}
	}
}