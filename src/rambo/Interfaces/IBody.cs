namespace rambo.Interfaces
{
    public interface IBody
    {
        string MessageType { get; }

        byte[] Content { get; }
    }
}