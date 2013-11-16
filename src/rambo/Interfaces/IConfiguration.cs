using System.Collections;
using System.Collections.Generic;

namespace rambo.Interfaces
{
    public interface IConfiguration
    {
        IConfigurationIndex Key { get; }
        IEnumerable<INode> Nodes { get; }
        ConfigurationState State { get; }
    }
}