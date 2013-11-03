namespace rambo.Interfaces
{
    public interface IMessage
    {
        IEnvelope Envelope { get; }

        IBody Body { get; }
    }
}