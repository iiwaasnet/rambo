using System;

namespace rambo.Messaging
{
	public enum FLeaseMessageType
	{
		Read,
		Write,
		AckRead,
		AckWrite,
		NackRead,
		NackWrite
	}

	public static class FLeaseMessageTypeExtensions
	{
		public static string ToMessageType(this FLeaseMessageType msgType)
		{
			switch (msgType)
			{
				case FLeaseMessageType.AckRead:
					return "ACKREAD";
				case FLeaseMessageType.AckWrite:
					return "ACKWRITE";
				case FLeaseMessageType.NackRead:
					return "NACKREAD";
				case FLeaseMessageType.NackWrite:
					return "NACKWRITE";
				case FLeaseMessageType.Read:
					return "READ";
				case FLeaseMessageType.Write:
					return "WRITE";
				default:
					throw new NotImplementedException(string.Format("{0}", msgType));
			}
		}

		public static FLeaseMessageType ToMessageType(this string msgType)
		{
			switch (msgType)
			{
				case "ACKREAD":
					return FLeaseMessageType.AckRead;
				case "ACKWRITE":
					return FLeaseMessageType.AckWrite;
				case "NACKREAD":
					return FLeaseMessageType.NackRead;
				case "NACKWRITE":
					return FLeaseMessageType.NackWrite;
				case "READ":
					return FLeaseMessageType.Read;
				case "WRITE":
					return FLeaseMessageType.Write;
				default:
					throw new NotImplementedException(string.Format("{0}", msgType));
			}
		}
	}
}