﻿using System.Collections.Concurrent;
using rambo.Interfaces;

namespace rambo.Implementation
{
    internal class CurrentOperation
    {
        internal IObservableAtomicValue<OperationType> Type { get; set; }
        internal IObservableAtomicValue<OperationPhase> Phase { get; set; }
        internal IPhaseNumber PhaseNumber { get; set; }
        internal IObservableConcurrentDictionary<IConfigurationIndex, IConfiguration> ConfigurationMap { get; set; }
        internal ConcurrentDictionary<int, INode> Accepted { get; set; }
        internal IObjectValue Value { get; set; }
    }
}