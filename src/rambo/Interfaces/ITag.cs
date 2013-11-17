namespace rambo.Interfaces
{
    /// <summary>
    /// Pair (n + 1; i)
    /// </summary>
    public interface ITag
    {
        void Increment();
        bool GreaterThan(ITag t);
        string Value { get; }
    }
}