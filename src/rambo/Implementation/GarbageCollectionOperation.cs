using System.Collections.Concurrent;
using rambo.Interfaces;

namespace rambo.Implementation
{
    internal class GarbageCollectionOperation
    {
        internal IObservableAtomicValue<OperationPhase> Phase { get; set; }
        internal IPhaseNumber PhaseNumber { get; set; }
        internal ConcurrentDictionary<IConfigurationIndex, IConfiguration> ConfigurationMap { get; set; }
        internal ConcurrentDictionary<int, INode> Accepted { get; set; }
        internal IConfigurationIndex Target { get; set; }
    }
}