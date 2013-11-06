using System;

namespace rambo.Messaging
{
	public interface IMessageHub : IDisposable
	{
		IListener Subscribe(IProcess subscriber);

		void Broadcast(IMessage message);

		void Send(IProcess recipient, IMessage message);
	}
}