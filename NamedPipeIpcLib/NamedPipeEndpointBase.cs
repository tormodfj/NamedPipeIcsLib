using System;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;

namespace NamedPipeIpcLib
{
	public abstract class NamedPipeEndpointBase<T> : INamedPipeEndpoint where T : PipeStream
	{
		protected const int BufferSize = 4096;

		private StreamWriter pipeWriter;

		protected NamedPipeEndpointBase(string pipeName)
		{
			if (pipeName == null)
			{
				throw new ArgumentNullException("pipeName");
			}
			PipeName = pipeName;
		}

		public event EventHandler<MessageReceivedEventArgs> MessageReceived;

		public string PipeName { get; private set; }

		protected bool StopRequested { get; private set; }

		protected T Pipe { get; private set; }

		protected abstract bool AutoFlushPipeWriter { get; }

		public void Start()
		{
			Pipe = CreateStream();
			ThreadPool.QueueUserWorkItem(state => ReadFromPipe());
		}

		public void Stop()
		{
			StopRequested = true;
			Pipe.Close();
			Pipe.Dispose();
		}

		public void Write(string message)
		{
			if (Pipe.IsConnected && Pipe.CanWrite)
			{
				if (pipeWriter == null)
				{
					pipeWriter = new StreamWriter(Pipe)
					{
						AutoFlush = AutoFlushPipeWriter
					};
				}

				pipeWriter.WriteLine(message);
			}
		}

		protected abstract T CreateStream();

		protected abstract void ReadFromPipe();

		protected virtual void OnMessageReceived(MessageReceivedEventArgs e)
		{
			if (MessageReceived != null)
			{
				MessageReceived(this, e);
			}
		}

		protected string ReadMessage()
		{
			var memoryStream = new MemoryStream();
			var buffer = new byte[BufferSize];

			do
			{
				int bytesRead = Pipe.Read(buffer, 0, buffer.Length);
				memoryStream.Write(buffer, 0, bytesRead);
			} while (!Pipe.IsMessageComplete);

			return Encoding.Default.GetString(memoryStream.ToArray());
		}
	}
}