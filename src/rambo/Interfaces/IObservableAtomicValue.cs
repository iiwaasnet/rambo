namespace rambo.Interfaces
{

    public interface IObservableAtomicValue<T> : IChangeNotifiable
    {
        void Set(T value);

        T Get();
    }
}