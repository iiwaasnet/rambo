﻿using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using rambo.Implementation.Messages;
using rambo.Interfaces;
using rambo.Messaging;

namespace rambo.Implementation
{
    public class ReaderWriter : IReaderWriter
    {
        private readonly object JoinAckEvent = new object();
        private readonly object ReadAckEvent = new object();
        private readonly object WriteAckEvent = new object();
        private readonly IObservableConcurrentDictionary<IConfigurationIndex, IConfiguration> configMap;
        private readonly INode creator;
        private readonly EventHandlerList eventHandlers;
        private readonly GarbageCollectionOperation gc;
        private readonly IMessageHub messageHub;
        private readonly CurrentOperation op;
        private readonly BlockingCollection<OperationRequest> operationQueue;
        private readonly IObservableConcurrentDictionary<INode, IPhaseNumber> phaseVector;
        private readonly IObservableCondition preJoinAck;
        private readonly IObservableCondition preOutSend;
        private readonly IObservableCondition preQueryFix;
        private readonly IObservableCondition prePropagationFix;
        private readonly IObservableCondition preReadAck;
        private readonly IObservableCondition preWriteAck;
        private readonly IMessageSerializer serializer;
        private readonly IObservableAtomicValue<NodeStatus> status;
        private readonly IObservableConcurrentDictionary<int, INode> world;
        private readonly IPhaseNumber localPhase;
        private ITag tag;
        private IObjectValue value;

        public ReaderWriter(INode creator,
                            IMessageHub messageHub,
                            IEnumerable<IConfiguration> configMap,
                            IMessageSerializer serializer)
        {
            var configurations = configMap.ToDictionary(c => c.Key, c => c);
            this.serializer = serializer;
            op = new CurrentOperation
                 {
                     Phase = new ObservableAtomicValue<OperationPhase>(OperationPhase.Idle),
                     Type = new ObservableAtomicValue<OperationType>(OperationType.Idle),
                     Accepted = new ObservableConcurrentDictionary<int, INode>(Enumerable.Empty<KeyValuePair<int, INode>>()),
                     ConfigurationMap = new ObservableConcurrentDictionary<IConfigurationIndex, IConfiguration>(configurations)
                 };
            gc = new GarbageCollectionOperation {Phase = new ObservableAtomicValue<OperationPhase>(OperationPhase.Idle)};
            this.creator = creator;
            this.configMap = new ObservableConcurrentDictionary<IConfigurationIndex, IConfiguration>(configurations);
            this.messageHub = messageHub;
            value = new ObjectValue {Value = 0};
            tag = new Tag(creator);
            localPhase = new PhaseNumber();
            operationQueue = new BlockingCollection<OperationRequest>(new ConcurrentQueue<OperationRequest>());
            eventHandlers = new EventHandlerList();
            status = new ObservableAtomicValue<NodeStatus>(NodeStatus.Idle);
            world = new ObservableConcurrentDictionary<int, INode>();
            phaseVector = new ObservableConcurrentDictionary<INode, IPhaseNumber>();

            preJoinAck = new ObservableCondition(() => status.Get() == NodeStatus.Active, new[] {status});
            preOutSend = new ObservableCondition(() => status.Get() == NodeStatus.Active, new[] {status});
            preQueryFix = new ObservableCondition(QueryFixCondition,
                                                  new IChangeNotifiable[] {status, op.Type, op.Phase, op.Accepted, op.ConfigurationMap});
            prePropagationFix = new ObservableCondition(PropagationFixCondition,
                                                        new IChangeNotifiable[] {status, op.Type, op.Phase, op.Accepted, op.ConfigurationMap});
            preReadAck = new ObservableCondition(() => status.Get() == NodeStatus.Active
                                                       && op.Type.Get() == OperationType.Read
                                                       && op.Phase.Get() == OperationPhase.Done,
                                                 new IChangeNotifiable[] {status, op.Type, op.Phase});
            preWriteAck = new ObservableCondition(() => status.Get() == NodeStatus.Active
                                                        && op.Type.Get() == OperationType.Write
                                                        && op.Phase.Get() == OperationPhase.Done,
                                                  new IChangeNotifiable[] {status, op.Type, op.Phase});
            new Thread(ProcessReadWriteRequests).Start();
            new Thread(OutJoinAck).Start();
            new Thread(OutSend).Start();
            new Thread(IntQueryFix).Start();
            new Thread(IntPropagationFix).Start();
            new Thread(OutReadAck).Start();
            new Thread(OutWriteAck).Start();
            var listener = messageHub.Subscribe(creator);
            listener.Where(m => m.Body.MessageType.ToMessageType() == MessageTypes.JoinRw)
                    .Subscribe(new MessageStreamListener(OnJoinReceived));
            listener.Where(m => m.Body.MessageType.ToMessageType() == MessageTypes.Gossip)
                    .Subscribe(new MessageStreamListener(OnGossipReceived));
        }

        private void OutWriteAck()
        {
            preWriteAck.Waitable.WaitOne();

            var handler = eventHandlers[ReadAckEvent] as WriteAckEventHandler;
            if (handler != null)
            {
                handler(creator);
            }
            op.Phase.Set(OperationPhase.Idle);
        }

        private void OutReadAck()
        {
            preReadAck.Waitable.WaitOne();

            var handler = eventHandlers[ReadAckEvent] as ReadAckEventHandler;
            if (handler != null)
            {
                handler(null, op.Value);
            }
            op.Phase.Set(OperationPhase.Idle);
        }

        private bool PropagationFixCondition()
        {
            var operationType = op.Type.Get();

            return status.Get() == NodeStatus.Active
                   && (operationType == OperationType.Read || operationType == OperationType.Write)
                   && op.Phase.Get() == OperationPhase.Propagation
                   && QuorumReached(op.ConfigurationMap, op.Accepted);
        }

        private bool QueryFixCondition()
        {
            var operationType = op.Type.Get();

            return status.Get() == NodeStatus.Active
                   && (operationType == OperationType.Read || operationType == OperationType.Write)
                   && op.Phase.Get() == OperationPhase.Query
                   && QuorumReached(op.ConfigurationMap, op.Accepted);
        }

        private bool QuorumReached(IEnumerable<KeyValuePair<IConfigurationIndex, IConfiguration>> configurationMap,
                                   IEnumerable<KeyValuePair<int, INode>> accepted)
        {
            return configurationMap
                .Where(c => c.Value.State == ConfigurationState.Active)
                .Select(c => c.Value)
                .All(c => QuorumReached(c, accepted));
        }

        private bool QuorumReached(IConfiguration config, IEnumerable<KeyValuePair<int, INode>> accepted)
        {
            return config
                       .Nodes
                       .Join(accepted, c => c.Id, a => a.Value.Id, (n, acc) => n.Id)
                       .Count() >= QuorumSize(config.Nodes);
        }

        private int QuorumSize(IEnumerable<INode> nodes)
        {
            return nodes.Count() / 2 + 1;
        }

        public void Read(IObjectId x)
        {
            operationQueue.Add(new OperationRequest {OpType = OperationType.Read, Id = x});
        }

        public void Write(IObjectId x, IObjectValue v)
        {
            operationQueue.Add(new OperationRequest
                               {
                                   OpType = OperationType.Write,
                                   Id = x,
                                   Value = v
                               });
        }

        public void Fail()
        {
            status.Set(NodeStatus.Failed);
        }

        /// <summary>
        /// Input join(rw)i
        /// </summary>
        /// <param name="i">Joiner</param>
        public void Join(INode i)
        {
            if (status.Get() == NodeStatus.Idle)
            {
                world[i.Id] = i;
                status.Set((creator.Equals(i) ? NodeStatus.Active : NodeStatus.Joining));
            }
        }

        /// <summary>
        /// recv(world; v; t; cm; snder-phase; rcver-phasei)j;i
        /// </summary>
        /// <param name="message"></param>
        private void OnGossipReceived(IMessage message)
        {
            if (!IsIdleOrFailed())
            {
                var gossip = serializer.Deserialize<Gossip>(message.Body.Content);
                status.Set(NodeStatus.Active);
                MergeWorld(world, gossip.World);
                if (gossip.Tag.GreaterThan(tag))
                {
                    value = gossip.Value;
                    tag = gossip.Tag;
                }
                UpdateConfigurationMap(configMap, gossip.Configurations);
                if (gossip.SenderPhase.Number > phaseVector[message.Envelope.Sender.Node].Number)
                {
                    phaseVector[message.Envelope.Sender.Node] = gossip.SenderPhase;
                }
                var opPhase = op.Phase.Get();
                if (opPhase == OperationPhase.Query || opPhase == OperationPhase.Propagation)
                {
                    if (gossip.ReceiverPhase.Number >= op.PhaseNumber.Number)
                    {
                        ExtendConfigurationMap(op.ConfigurationMap, gossip.Configurations);
                        if (Usable(op.ConfigurationMap))
                        {
                            op.Accepted[message.Envelope.Sender.Node.Id] = message.Envelope.Sender.Node;
                        }
                        else
                        {
                            localPhase.Increment();
                            op.Accepted = new ObservableConcurrentDictionary<int, INode>();
                            op.ConfigurationMap = configMap;
                        }
                    }
                    if (gossip.ReceiverPhase.Number >= gc.PhaseNumber.Number)
                    {
                        gc.Accepted[message.Envelope.Sender.Node.Id] = message.Envelope.Sender.Node;
                    }
                }
            }
        }

        private bool Usable(IObservableConcurrentDictionary<IConfigurationIndex, IConfiguration> configurationMap)
        {
            var gc = configurationMap.Where(entry => entry.Value.State == ConfigurationState.GCed);
            var active = configurationMap.Where(entry => entry.Value.State == ConfigurationState.Active);

            return MonitonicallyIncreasing(gc.Select(c => c.Key), active.Select(c => c.Key));
        }

        private bool MonitonicallyIncreasing(IEnumerable<IConfigurationIndex> gc, IEnumerable<IConfigurationIndex> active)
        {
            var indices = gc.OrderBy(i => i.Id).Concat(active.OrderBy(i => i.Id));

            var first = indices.First();

            foreach (var next in indices.Skip(1))
            {
                if (first.Id != next.Id + 1)
                {
                    return false;
                }
                first = next;
            }

            return true;
        }

        private void ExtendConfigurationMap(IObservableConcurrentDictionary<IConfigurationIndex, IConfiguration> localConfiguration,
                                            IEnumerable<KeyValuePair<IConfigurationIndex, IConfiguration>> senderConfig)
        {
            foreach (var configuration in senderConfig)
            {
                if (!localConfiguration.ContainsKey(configuration.Key))
                {
                    localConfiguration[configuration.Key] = configuration.Value;
                }
            }
        }

        private void UpdateConfigurationMap(IObservableConcurrentDictionary<IConfigurationIndex, IConfiguration> localConfiguration,
                                            IEnumerable<KeyValuePair<IConfigurationIndex, IConfiguration>> senderConfig)
        {
            foreach (var configuration in senderConfig)
            {
                if (configuration.Value.State == ConfigurationState.GCed || localConfiguration[configuration.Key].State == ConfigurationState.GCed)
                {
                    localConfiguration[configuration.Key] = new GarbageCollectedConfiguration(configuration.Key);
                }
                else if (configuration.Value.State == ConfigurationState.Active)
                {
                    localConfiguration[configuration.Key] = configuration.Value;
                }
            }
        }

        private void MergeWorld(IObservableConcurrentDictionary<int, INode> localWorld, IEnumerable<INode> senderWorld)
        {
            foreach (var node in senderWorld)
            {
                localWorld[node.Id] = node;
            }
        }

        /// <summary>
        /// send(world; v; t; cm; snder-phase; rcver-phasei)i;j
        /// </summary>
        private void OutSend()
        {
            while (true)
            {
                preOutSend.Waitable.WaitOne();

                foreach (var node in world.Values)
                {
                    messageHub.Send(creator,
                                    new GossipMessage(creator,
                                                      new Gossip
                                                      {
                                                          Value = value,
                                                          Tag = tag,
                                                          World = world.Values.ToArray(),
                                                          SenderPhase = localPhase,
                                                          ReceiverPhase = phaseVector[node],
                                                          Configurations = configMap
                                                      },
                                                      serializer));
                }
            }
        }

        // TODO: Think of queueing recon() and fail() operations to be executed sequentually
        private void ProcessReadWriteRequests()
        {
            foreach (var operationRequest in operationQueue.GetConsumingEnumerable())
            {
                switch (operationRequest.OpType)
                {
                    case OperationType.Read:
                        InternalRead(operationRequest.Id);
                        break;
                    case OperationType.Write:
                        InternalWrite(operationRequest.Id, operationRequest.Value);
                        break;
                }
            }

            operationQueue.Dispose();
        }

        /// <summary>
        /// Input recv(join)j;i
        /// </summary>
        /// <param name="obj"></param>
        private void OnJoinReceived(IMessage obj)
        {
            if (!IsIdleOrFailed())
            {
                world[obj.Envelope.Sender.Node.Id] = obj.Envelope.Sender.Node;
            }
        }

        private bool IsIdleOrFailed()
        {
            return new[] {NodeStatus.Failed, NodeStatus.Idle}.Contains(status.Get());
        }

        /// <summary>
        /// Output join-ack(rw)i
        /// </summary>
        private void OutJoinAck()
        {
            preJoinAck.Waitable.WaitOne();

            var handler = eventHandlers[JoinAckEvent] as JoinAckEventHandler;
            if (handler != null)
            {
                handler(creator);
            }
        }

        private void InternalRead(IObjectId x)
        {
            if (!IsIdleOrFailed())
            {
                localPhase.Increment();
                op.PhaseNumber = localPhase;
                op.ConfigurationMap = configMap;
                op.Accepted = new ObservableConcurrentDictionary<int, INode>();
                op.Phase.Set(OperationPhase.Query);
                op.Type.Set(OperationType.Read);
            }
        }

        private void InternalWrite(IObjectId x, IObjectValue v)
        {
            if (!IsIdleOrFailed())
            {
                localPhase.Increment();
                op.PhaseNumber = localPhase;
                op.Accepted = new ObservableConcurrentDictionary<int, INode>();
                op.Value = v;
                op.ConfigurationMap = configMap;
                op.Phase.Set(OperationPhase.Query);
                op.Type.Set(OperationType.Write);
            }
        }

        /// <summary>
        /// query-fix i
        /// </summary>
        private void IntQueryFix()
        {
            preQueryFix.Waitable.WaitOne();

            if (op.Type.Get() == OperationType.Read)
            {
                op.Value = value;
            }
            else
            {
                value = op.Value;
                tag.Increment();
                localPhase.Increment();
            }
            op.Accepted.Set(Enumerable.Empty<KeyValuePair<int, INode>>());
            op.ConfigurationMap.Set(configMap);
            op.Phase.Set(OperationPhase.Propagation);
        }

        /// <summary>
        /// prop-fix i
        /// </summary>
        private void IntPropagationFix()
        {
            prePropagationFix.Waitable.WaitOne();

            op.Phase.Set(OperationPhase.Done);
        }

        public event ReadAckEventHandler ReadAck
        {
            add { eventHandlers.AddHandler(ReadAckEvent, value); }
            remove { eventHandlers.RemoveHandler(ReadAckEvent, value); }
        }

        public event WriteAckEventHandler WriteAck
        {
            add { eventHandlers.AddHandler(WriteAckEvent, value); }
            remove { eventHandlers.RemoveHandler(WriteAckEvent, value); }
        }

        public event JoinAckEventHandler JoinAck
        {
            add { eventHandlers.AddHandler(JoinAckEvent, value); }
            remove { eventHandlers.RemoveHandler(JoinAckEvent, value); }
        }
    }
}