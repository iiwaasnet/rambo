using System;
using System.Threading;
using rambo.Interfaces;

namespace rambo.Implementation
{
    public class Precondition : IPrecondition
    {
        private readonly AutoResetEvent waitHandle;
        private readonly IObservableCondition condition;

        public Precondition(IObservableCondition condition)
        {
            waitHandle = new AutoResetEvent(false);
            this.condition = condition;
            this.condition.Changed += ConditionChanged;
        }

        private void ConditionChanged()
        {
            if (condition.Evaluate())
            {
                waitHandle.Set();
            }
        }

        public bool Wait(TimeSpan timeout)
        {
            return waitHandle.WaitOne(timeout);
        }
    }
}