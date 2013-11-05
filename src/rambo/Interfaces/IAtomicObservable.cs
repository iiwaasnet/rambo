namespace rambo.Interfaces
{

    public interface IAtomicObservable<T> : IChangeNotifiable
    {
        void Set(T value);

        T Get();
    }
}