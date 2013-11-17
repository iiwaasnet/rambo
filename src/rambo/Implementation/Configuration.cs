using System.Collections.Generic;
using rambo.Interfaces;

namespace rambo.Implementation
{
    public class Configuration : IConfiguration
    {
        public Configuration(IConfigurationIndex key, IEnumerable<INode> nodes)
        {
            Nodes = nodes;
            Key = key;
        }

        public IConfigurationIndex Key { get; private set; }
        public IEnumerable<INode> Nodes { get; private set; }

        public ConfigurationState State
        {
            get { return ConfigurationState.Active; }
        }
    }
}