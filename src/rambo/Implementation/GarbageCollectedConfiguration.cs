using System.Collections.Generic;
using System.Linq;
using rambo.Interfaces;

namespace rambo.Implementation
{
    public class GarbageCollectedConfiguration : IConfiguration
    {
        public GarbageCollectedConfiguration(IConfigurationIndex key)
        {
            State = ConfigurationState.GCed;
            Key = key;
            Nodes = Enumerable.Empty<INode>();
        }

        public IEnumerable<INode> Nodes { get; private set; }

        public ConfigurationState State { get; private set; }

        public IConfigurationIndex Key { get; private set; }
    }
}