using System;
using System.ComponentModel;
using System.Threading;
using rambo.Interfaces;

namespace rambo.Implementation
{
    public class AtomicObservable<T> : IAtomicObservable<T>
    {
        private readonly EventHandlerList eventHandlers;
        private readonly object ChangedEvent = new object();
        private Func<T> value;

        public AtomicObservable(T value)
        {
            eventHandlers = new EventHandlerList();
            this.value = () => value;
        }

        public void Set(T value)
        {
            Interlocked.Exchange(ref this.value, this.value = () => value);

            var handler = eventHandlers[ChangedEvent] as ChangedEventHandler;

            if (handler != null)
            {
                handler();
            }
        }

        public T Get()
        {
            return value();
        }

        public event ChangedEventHandler Changed
        {
            add { eventHandlers.AddHandler(ChangedEvent, value); }
            remove { eventHandlers.RemoveHandler(ChangedEvent, value); }
        }
    }
}