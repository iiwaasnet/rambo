using System;
using System.ComponentModel;
using System.Threading;
using rambo.Interfaces;

namespace rambo.Implementation
{
    public class ObservableAtomicValue<T> : IObservableAtomicValue<T>
    {
        private readonly EventHandlerList eventHandlers;
        private readonly object ChangedEvent = new object();
        private Func<T> value;

        public ObservableAtomicValue(T value)
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