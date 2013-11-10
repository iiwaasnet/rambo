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
    public class ReaderWriter : IReaderWriter
    {
        private readonly IAtomicObservable<NodeStatus> status;
        private readonly ConcurrentDictionary<int, INode> world;
        private readonly INode creator;
        private IObjectValue value;
        private ITag tag;
        private IPhase localPhase;
        private readonly ConcurrentDictionary<INode, IPhase> phaseVector;
        private readonly ConcurrentDictionary<IPhase, IConfiguration> configMap;
        private readonly EventHandlerList eventHandlers;
        private readonly IObservableCondition preJoinAck;
        private readonly object ReadAckEvent = new object();
        private readonly object WriteAckEvent = new object();
        private readonly object JoinAckEvent = new object();

        public ReaderWriter(INode creator, IMessageHub messageHub)
        {
            this.creator = creator;
            eventHandlers = new EventHandlerList();
            status = new AtomicObservable<NodeStatus>(NodeStatus.Idle);
            world = new ConcurrentDictionary<int, INode>();
            preJoinAck = new ObservableCondition(() => status.Get() == NodeStatus.Active, new[] {status});
            new Thread(OutJoinAck).Start();
            var listener = messageHub.Subscribe(creator);
            listener.Where(m => m.Body.MessageType.ToMessageType() == MessageTypes.JoinRw)
                    .Subscribe(new MessageStreamListener(OnJoinReceived));
        }

        /// <summary>
        /// Input recv(join)j;i
        /// </summary>
        /// <param name="obj"></param>
        private void OnJoinReceived(IMessage obj)
        {
            if (!new[] {NodeStatus.Failed, NodeStatus.Idle}.Contains(status.Get()))
            {
                world[obj.Envelope.Sender.Node.Id] = obj.Envelope.Sender.Node;
            }
        }

        /// <summary>
        /// Output join-ack(rw)i
        /// </summary>
        private void OutJoinAck()
        {
            preJoinAck.Waitable.WaitOne();

            var handler = eventHandlers[JoinAckEvent] as JoinAckEventHandler;
            if(handler != null)
            {
                handler(creator);
            }
        }

        public void Read(IObjectId x)
        {
            throw new System.NotImplementedException();
        }

        public void Write(IObjectId x, IObjectValue v)
        {
            throw new System.NotImplementedException();
        }

        public void Fail()
        {
            throw new System.NotImplementedException();
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