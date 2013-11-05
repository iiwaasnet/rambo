using System.Threading;

namespace rambo.Interfaces
{
    public interface IObservableCondition
    {
        WaitHandle Waitable { get; }
    }
}