namespace rambo.Messaging
{
	public class ForwardRequest
	{
		public IProcess Recipient { get; set; }

		public IMessage Message { get; set; }
	}
}