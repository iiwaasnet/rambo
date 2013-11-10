using System.Collections.Concurrent;
using rambo.Interfaces;

namespace rambo.Implementation
{
    internal class CurrentOperation
    {
        internal IAtomicObservable<OperationType> Type { get; set; }
        internal IAtomicObservable<OperationPhase> Phase { get; set; }
        internal IPhaseNumber PhaseNumber { get; set; }
        internal ConcurrentDictionary<IConfigurationIndex, IConfiguration> ConfigurationMap { get; set; }
        internal ConcurrentDictionary<int, INode> Accepted { get; set; }
        internal IObjectValue Value { get; set; }
    }
}