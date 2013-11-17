using System.Collections.Generic;
using System.Linq;
using rambo.Interfaces;

namespace rambo.Implementation
{
    public class GarbageCollectedConfiguration : IConfiguration
    {
        public GarbageCollectedConfiguration(IConfigurationIndex key)
        {
            Key = key;
        }

        public IEnumerable<INode> Nodes
        {
            get { return Enumerable.Empty<INode>(); }
        }

        public ConfigurationState State
        {
            get { return ConfigurationState.GCed; }
        }

        public IConfigurationIndex Key { get; private set; }
    }
}