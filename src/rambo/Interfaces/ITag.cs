namespace rambo.Interfaces
{
    public interface ITag
    {
        bool GreaterThan(ITag t);
        string Tag { get; }
    }
}