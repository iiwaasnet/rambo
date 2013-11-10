using rambo.Interfaces;

namespace rambo.Messaging
{
	public class Sender : ISender
	{
		public INode Node { get; set; }
	}
}