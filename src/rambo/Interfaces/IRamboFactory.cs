namespace rambo.Interfaces
{
    public interface IRamboFactory
    {
        IRambo Build(INode node);
    }
}