using System;

namespace rambo.Implementation.Messages
{
    public enum MessageTypes
    {
        JoinRw
    }

    public static class MessageTypesExtensions
    {
        private const string JoinRwMessage = "JOINRW";

        public static string ToMessageType(this MessageTypes msgType)
        {
            switch (msgType)
            {
                case MessageTypes.JoinRw:
                    return JoinRwMessage;
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

                default:
                    throw new NotImplementedException(string.Format("{0}", msgType));
            }
        }
    }
}