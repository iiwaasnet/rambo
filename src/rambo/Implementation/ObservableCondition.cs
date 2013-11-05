using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using rambo.Interfaces;

namespace rambo.Implementation
{
    public class ObservableCondition : IObservableCondition
    {
        private readonly EventHandlerList eventHandlers;
        private readonly object ChangedEvent = new object();
        private readonly Func<bool> condition;

        public ObservableCondition(Func<bool> condition, IEnumerable<IChangeNotifiable> members)
        {
            this.condition = condition;
            eventHandlers = new EventHandlerList();

            BindEventHandlers(members);
        }

        private void BindEventHandlers(IEnumerable<IChangeNotifiable> members)
        {
            foreach (var changeNotifiable in members)
            {
                changeNotifiable.Changed += OnNotifiableChanged;
            }
        }

        private void OnNotifiableChanged()
        {
            var eventHandler = eventHandlers[ChangedEvent] as ChangedEventHandler;

            if (eventHandler != null)
            {
                eventHandler();
            }
        }

        public bool Evaluate()
        {
            return condition();
        }

        public event ChangedEventHandler Changed
        {
            add { eventHandlers.AddHandler(ChangedEvent, value); }
            remove { eventHandlers.RemoveHandler(ChangedEvent, value); }
        }
    }
}