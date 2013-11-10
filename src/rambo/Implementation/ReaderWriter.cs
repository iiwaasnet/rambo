using System.Collections.Concurrent;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using rambo.Implementation.Messages;
using rambo.Interfaces;
using rambo.Messaging;

namespace rambo.Implementation
{
    public class ReaderWriter<T> : IReaderWriter
    {
        private readonly IAtomicObservable<NodeStatus> status;
        private readonly ConcurrentDictionary<int, INode> world;
        private readonly INode creator;
        private IObjectValue<T> value;
        private ITag tag;
        private readonly IPhaseNumber localPhase;
        private readonly ConcurrentDictionary<INode, IPhaseNumber> phaseVector;
        private readonly ConcurrentDictionary<IConfigurationIndex, IConfiguration> configMap;
        private readonly EventHandlerList eventHandlers;
        private readonly IObservableCondition preJoinAck;
        private readonly object ReadAckEvent = new object();
        private readonly object WriteAckEvent = new object();
        private readonly object JoinAckEvent = new object();
        private readonly CurrentOperation op;
        private readonly BlockingCollection<OperationRequest> operationQueue;

        public ReaderWriter(INode creator,
                            IMessageHub messageHub,
                            ConcurrentDictionary<IConfigurationIndex, IConfiguration> configMap)
        {
            op = new CurrentOperation
                 {
                     Phase = new AtomicObservable<OperationPhase>(OperationPhase.Idle)
                 };
            this.creator = creator;
            this.configMap = configMap;
            value = new ObjectValue<T>
                    {
                        Value = default(T)
                    };
            tag = new Tag(creator);
            localPhase = new PhaseNumber();
            operationQueue = new BlockingCollection<OperationRequest>(new ConcurrentQueue<OperationRequest>());
            eventHandlers = new EventHandlerList();
            status = new AtomicObservable<NodeStatus>(NodeStatus.Idle);
            world = new ConcurrentDictionary<int, INode>();

            preJoinAck = new ObservableCondition(() => status.Get() == NodeStatus.Active, new[] {status});
            new Thread(ProcessReadWriteREquests).Start();
            new Thread(OutJoinAck).Start();
            var listener = messageHub.Subscribe(creator);
            listener.Where(m => m.Body.MessageType.ToMessageType() == MessageTypes.JoinRw)
                    .Subscribe(new MessageStreamListener(OnJoinReceived));
        }

        // TODO: Think of queueing recon() and fail() operations to be executed sequentually
        private void ProcessReadWriteREquests()
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
                op.ConfigurationMap =
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