using rambo.Interfaces;
using rambo.Messaging;

namespace rambo.Implementation.Messages
{
    public class JoinRwMessage : IMessage
    {
        public JoinRwMessage(INode i)
        {
            Envelope = new Envelope {Sender = new Sender {Process = i}};
            Body = new Body
                   {
                       MessageType = MessageTypes.JoinRw.ToMessageType(),
                       Content = new byte[0]
                   };
        }

        public IEnvelope Envelope { get; private set; }
        public IBody Body { get; private set; }
    }
}