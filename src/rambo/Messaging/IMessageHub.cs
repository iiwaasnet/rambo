using System;
using rambo.Interfaces;

namespace rambo.Messaging
{
	public interface IMessageHub : IDisposable
	{
		IListener Subscribe(INode subscriber);

		void Broadcast(IMessage message);

		void Send(INode recipient, IMessage message);
	}
}