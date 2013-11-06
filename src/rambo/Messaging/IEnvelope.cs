namespace rambo.Messaging
{
	public interface IEnvelope
	{
		ISender Sender { get; }
	}
}