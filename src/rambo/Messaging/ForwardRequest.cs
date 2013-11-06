using rambo.Interfaces;

namespace rambo.Messaging
{
	public class ForwardRequest
	{
		public INode Recipient { get; set; }

		public IMessage Message { get; set; }
	}
}