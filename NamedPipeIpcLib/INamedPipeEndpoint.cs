using System;

namespace NamedPipeIpcLib
{
	public interface INamedPipeEndpoint
	{
		event EventHandler<MessageReceivedEventArgs> MessageReceived;

		string PipeName { get; }

		void Start();

		void Stop();

		void Write(string message);
	}
}