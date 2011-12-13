using System;
using System.Diagnostics;
using System.IO.Pipes;

namespace NamedPipeIpcLib
{
	public class NamedPipeServer : NamedPipeEndpointBase<NamedPipeServerStream>
	{
		public NamedPipeServer(string pipeName)
			: base(pipeName)
		{
		}

		protected override bool AutoFlushPipeWriter
		{
			get { return true; }
		}

		protected override NamedPipeServerStream CreateStream()
		{
			var stream = new NamedPipeServerStream(
				pipeName: PipeName, 
				direction: PipeDirection.InOut, 
				maxNumberOfServerInstances: 1, 
				transmissionMode: PipeTransmissionMode.Message, 
				options: PipeOptions.Asynchronous, 
				inBufferSize: BufferSize, 
				outBufferSize: BufferSize);

			return stream;
		}

		protected override void ReadFromPipe()
		{
			try
			{
				while (Pipe != null && !StopRequested)
				{
					if (!Pipe.IsConnected)
					{
						Pipe.WaitForConnection();
					}

					string message = ReadMessage();
					OnMessageReceived(new MessageReceivedEventArgs(message));
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.ToString());
			}
		}
	}
}