using System;
using System.Collections.Concurrent;
using System.Threading;

namespace rambo.Messaging
{
	public class Listener : IListener
	{
		private readonly ConcurrentDictionary<IObserver<IMessage>, object> observers;
		private readonly BlockingCollection<IMessage> messages;
		private readonly Thread notifyThread;

		public Listener(IProcess subscriber)
		{
			Subscriber = subscriber;
			observers = new ConcurrentDictionary<IObserver<IMessage>, object>();
			messages = new BlockingCollection<IMessage>(new ConcurrentQueue<IMessage>());
			notifyThread = new Thread(ForwardMessages);
		}

		public void Notify(IMessage message)
		{
			messages.Add(message);
		}

		private void ForwardMessages()
		{
			foreach (var message in messages.GetConsumingEnumerable())
			{
				foreach (var observer in observers)
				{
					observer.Key.OnNext(message);
				}
			}

			messages.Dispose();
		}

		public IDisposable Subscribe(IObserver<IMessage> observer)
		{
			observers[observer] = null;

			return new Unsubscriber(observers, observer);
		}

		public void Start()
		{
			notifyThread.Start();
		}

		public void Stop()
		{
			foreach (var observer in observers)
			{
				observer.Key.OnCompleted();
			}
			messages.CompleteAdding();
		}

		public IProcess Subscriber { get; private set; }

		private class Unsubscriber : IDisposable
		{
			private readonly ConcurrentDictionary<IObserver<IMessage>, object> observers;
			private readonly IObserver<IMessage> observer;

			public Unsubscriber(ConcurrentDictionary<IObserver<IMessage>, object> observers, IObserver<IMessage> observer)
			{
				this.observer = observer;
				this.observers = observers;
			}

			public void Dispose()
			{
				if (observer != null)
				{
					object val;
					observers.TryRemove(observer, out val);
				}
			}
		}
	}
}