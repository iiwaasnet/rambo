namespace rambo.Interfaces
{
    public interface ITag
    {
        void Increment();
        bool GreaterThan(ITag t);
        string Value { get; }
    }
}