namespace rambo.Messaging
{
	public interface IBody
	{
		string MessageType { get; }

		byte[] Content { get; }
	}
}