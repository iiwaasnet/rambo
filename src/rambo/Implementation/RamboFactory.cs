using rambo.Interfaces;
using rambo.Messaging;

namespace rambo.Implementation
{
    public class RamboFactory : IRamboFactory
    {
        private readonly IMessageHub messageHub;
        private readonly IConfiguration initialConfig;
        private readonly IMessageSerializer messageSerializer;

        public RamboFactory(IMessageHub messageHub,
                            IConfiguration initialConfig,
                            IMessageSerializer messageSerializer)
        {
            this.messageHub = messageHub;
            this.initialConfig = initialConfig;
            this.messageSerializer = messageSerializer;
        }

        public IRambo Build(INode node)
        {
            return new Rambo(node, messageHub, new[] {initialConfig}, messageSerializer);
        }
    }
}