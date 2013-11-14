using rambo.Interfaces;
using rambo.Messaging;

namespace rambo.Implementation.Messages
{
    public class GossipMessage : IMessage
    {
        public GossipMessage(INode sender, Gossip gossip, IMessageSerializer serializer)
        {
            Envelope = new Envelope {Sender = new Sender {Node = sender}};
            Body = new Body
                   {
                       MessageType = MessageTypes.Gossip.ToMessageType(),
                       Content = serializer.Serialize(gossip)
                   };
        }

        public IEnvelope Envelope { get; private set; }
        public IBody Body { get; private set; }
    }
}