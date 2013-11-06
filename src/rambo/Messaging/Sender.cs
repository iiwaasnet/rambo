using rambo.Interfaces;

namespace rambo.Messaging
{
	public class Sender : ISender
	{
		public INode Process { get; set; }
	}
}