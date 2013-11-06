using System;

namespace rambo.Messaging
{
	public interface IListener : IObservable<IMessage>
	{
		void Start();

		void Stop();
	}
}