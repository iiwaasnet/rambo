using System.Collections.Concurrent;
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
        private readonly IAtomicObservable<NodeStatus> status;
        private readonly ConcurrentDictionary<int, INode> world;
        private readonly INode creator;
        private IObjectValue value;
        private readonly ITag tag;
        private readonly IPhaseNumber localPhase;
        private readonly ConcurrentDictionary<INode, IPhaseNumber> phaseVector;
        private readonly ConcurrentDictionary<IConfigurationIndex, IConfiguration> configMap;
        private readonly EventHandlerList eventHandlers;
        private readonly IMessageHub messageHub;
        private readonly IObservableCondition preJoinAck;
        private readonly IObservableCondition preOutSend;
        private readonly object ReadAckEvent = new object();
        private readonly object WriteAckEvent = new object();
        private readonly object JoinAckEvent = new object();
        private readonly CurrentOperation op;
        private readonly BlockingCollection<OperationRequest> operationQueue;
        private readonly IMessageSerializer serializer;

        public ReaderWriter(INode creator,
                            IMessageHub messageHub,
                            ConcurrentDictionary<IConfigurationIndex, IConfiguration> configMap,
                            IMessageSerializer serializer)
        {
            this.serializer = serializer;
            op = new CurrentOperation
                 {
                     Phase = new AtomicObservable<OperationPhase>(OperationPhase.Idle)
                 };
            this.creator = creator;
            this.configMap = configMap;
            this.messageHub = messageHub;
            value = new ObjectValue {Value = 0};
            tag = new Tag(creator);
            localPhase = new PhaseNumber();
            operationQueue = new BlockingCollection<OperationRequest>(new ConcurrentQueue<OperationRequest>());
            eventHandlers = new EventHandlerList();
            status = new AtomicObservable<NodeStatus>(NodeStatus.Idle);
            world = new ConcurrentDictionary<int, INode>();

            preJoinAck = new ObservableCondition(() => status.Get() == NodeStatus.Active, new[] {status});
            preOutSend = new ObservableCondition(() => status.Get() == NodeStatus.Active, new[] {status});
            new Thread(ProcessReadWriteRequests).Start();
            new Thread(OutJoinAck).Start();
            new Thread(OutSend).Start();
            var listener = messageHub.Subscribe(creator);
            listener.Where(m => m.Body.MessageType.ToMessageType() == MessageTypes.JoinRw)
                    .Subscribe(new MessageStreamListener(OnJoinReceived));
            listener.Where(m => m.Body.MessageType.ToMessageType() == MessageTypes.Gossip)
                    .Subscribe(new MessageStreamListener(OnGossipReceived));
        }

        /// <summary>
        /// recv(world; v; t; cm; snder-phase; rcver-phasei)j;i
        /// </summary>
        /// <param name="obj"></param>
        private void OnGossipReceived(IMessage obj)
        {
            if (!IsIdleOrFailed())
            {
                var gossip = serializer.Deserialize<Gossip>(obj.Body.Content);
                status.Set(NodeStatus.Active);
                MergeWorld(world, gossip.World);
            }
        }

        private void MergeWorld(IDictionary<int, INode> localWorld, IEnumerable<INode> senderWorld)
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
                op.Accepted = new ConcurrentDictionary<int, INode>();
                op.Phase.Set(OperationPhase.Query);
                op.Type.Set(OperationType.Read);
            }
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

        private void InternalWrite(IObjectId x, IObjectValue v)
        {
            if (!IsIdleOrFailed())
            {
                localPhase.Increment();
                op.PhaseNumber = localPhase;
                op.Accepted = new ConcurrentDictionary<int, INode>();
                op.Value = v;
                op.ConfigurationMap = configMap;
                op.Phase.Set(OperationPhase.Query);
                op.Type.Set(OperationType.Write);
            }
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