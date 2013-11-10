using rambo.Interfaces;

namespace rambo.Messaging
{
	public interface ISender
	{
		INode Node { get; }
	}
}