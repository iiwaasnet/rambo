namespace rambo.Interfaces
{
    public interface INode
    {
        int Id { get; }

        bool Equals(INode other);
    }
}