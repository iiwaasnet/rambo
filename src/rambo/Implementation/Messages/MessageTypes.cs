using System;

namespace rambo.Implementation.Messages
{
    public enum MessageTypes
    {
        JoinRw,
        Gossip
    }

    public static class MessageTypesExtensions
    {
        private const string JoinRwMessage = "JOINRW";
        private const string GossipMessage = "GOSSIP";

        public static string ToMessageType(this MessageTypes msgType)
        {
            switch (msgType)
            {
                case MessageTypes.JoinRw:
                    return JoinRwMessage;
                case MessageTypes.Gossip:
                    return GossipMessage;
                default:
                    throw new NotImplementedException(string.Format("{0}", msgType));
            }
        }

        public static MessageTypes ToMessageType(this string msgType)
        {
            switch (msgType)
            {
                case JoinRwMessage:
                    return MessageTypes.JoinRw;
                case GossipMessage:
                    return MessageTypes.Gossip;
                default:
                    throw new NotImplementedException(string.Format("{0}", msgType));
            }
        }
    }
}