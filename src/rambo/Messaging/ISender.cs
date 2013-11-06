using rambo.Interfaces;

namespace rambo.Messaging
{
	public interface ISender
	{
		INode Process { get; }
	}
}