using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using rambo.Interfaces;

namespace rambo.Implementation
{
    public class ObservableConcurrentDictionary<K, V> : IObservableConcurrentDictionary<K, V>
    {
        private ConcurrentDictionary<K, V> storage;
        private readonly EventHandlerList eventHandlers;
        private readonly object ChangedEvent = new object();

        public ObservableConcurrentDictionary()
        {
            eventHandlers = new EventHandlerList();
            storage = new ConcurrentDictionary<K, V>();
        }

        public ObservableConcurrentDictionary(IEnumerable<KeyValuePair<K, V>> collection)
        {
            eventHandlers = new EventHandlerList();
            storage = new ConcurrentDictionary<K, V>(collection);
        }

        public int Count()
        {
            return storage.Count;
        }

        public void Set(IEnumerable<KeyValuePair<K, V>> collection)
        {
            storage = new ConcurrentDictionary<K, V>(collection);
            OnChanged();
        }

        private void OnChanged()
        {
            var handler = eventHandlers[ChangedEvent] as ChangedEventHandler;

            if (handler != null)
            {
                handler();
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<KeyValuePair<K, V>> GetEnumerator()
        {
            return storage.GetEnumerator();
        }

        public bool ContainsKey(K key)
        {
            return storage.ContainsKey(key);
        }

        public event ChangedEventHandler Changed
        {
            add { eventHandlers.AddHandler(ChangedEvent, value); }
            remove { eventHandlers.RemoveHandler(ChangedEvent, value); }
        }

        public IEnumerable<V> Values
        {
            get { return storage.Values; }
        }

        public V this[K key]
        {
            get { return storage[key]; }
            set
            {
                storage[key] = value;
                OnChanged();
            }
        }
    }
}