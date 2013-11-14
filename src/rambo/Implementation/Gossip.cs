using System.Collections.Generic;
using rambo.Interfaces;

namespace rambo.Implementation
{
    public class Gossip
    {
        public IEnumerable<INode> World { get; set; }
        public IObjectValue Value { get; set; }
        public ITag Tag { get; set; }
        public IDictionary<IConfigurationIndex, IConfiguration> Configurations { get; set; }
        public IPhaseNumber SenderPhase { get; set; }
        public IPhaseNumber ReceiverPhase { get; set; }
    }
}