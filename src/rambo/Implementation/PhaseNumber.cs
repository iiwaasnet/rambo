using System.Threading;
using rambo.Interfaces;

namespace rambo.Implementation
{
    public class PhaseNumber : IPhaseNumber
    {
        private int number;

        public PhaseNumber()
        {
            number = 0;
        }

        public void Increment()
        {
            Interlocked.Increment(ref number);
        }

        public int Number
        {
            get { return number; }
        }
    }
}