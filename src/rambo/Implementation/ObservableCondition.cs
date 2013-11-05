using System;
using System.Collections.Generic;
using System.Threading;
using rambo.Interfaces;

namespace rambo.Implementation
{
    public class ObservableCondition : IObservableCondition
    {
        private readonly Func<bool> condition;
        private readonly AutoResetEvent waitHandle;

        public ObservableCondition(Func<bool> condition, IEnumerable<IChangeNotifiable> members)
        {
            this.condition = condition;
            waitHandle = new AutoResetEvent(false);

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
            if (condition())
            {
                waitHandle.Set();
            }
            else
            {
                waitHandle.Reset();
            }
        }

        public WaitHandle Waitable
        {
            get { return waitHandle; }
        }
    }
}