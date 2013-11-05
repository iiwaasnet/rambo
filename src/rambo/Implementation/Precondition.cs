using System.Threading;
using rambo.Interfaces;

namespace rambo.Implementation
{
    public class Precondition : IPrecondition
    {
        
        private readonly IObservableCondition condition;

        public Precondition(IObservableCondition condition)
        {
            
            this.condition = condition;
            this.condition.Changed += ConditionChanged;
        }

        private void ConditionChanged()
        {
            
        }

        public WaitHandle Waitable
        {
            get { return waitHandle; }
        }
    }
}