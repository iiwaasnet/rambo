using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using rambo.Implementation.Messages;
using rambo.Interfaces;
using rambo.Messaging;

namespace rambo.Implementation
{
    public class Joiner : IJoiner
    {
        private readonly IReaderWriter readerWriter;
        private readonly EventHandlerList eventHandlers;
        private readonly IRecon recon;
        private readonly IAtomicObservable<NodeStatus> status;
        private readonly IAtomicObservable<NodeStatus> reconStatus;
        private readonly IAtomicObservable<NodeStatus> rwStatus;
        private readonly IAtomicObservable<IEnumerable<INode>> hints;
        private readonly IObservableCondition preJoinRw;
        private readonly IObservableCondition preJoinRecon;
        private readonly IObservableCondition preJoin;
        private readonly IObservableCondition preJoinAck;
        private readonly INode i;
        private readonly IMessageHub messageHub;
        private readonly object JoinAckEvent = new object();

        public Joiner(INode i, IReaderWriter readerWriter, IRecon recon, IMessageHub messageHub)
        {
            this.i = i;
            this.messageHub = messageHub;
            eventHandlers = new EventHandlerList();
            status = new AtomicObservable<NodeStatus>(NodeStatus.Idle);
            reconStatus = new AtomicObservable<NodeStatus>(NodeStatus.Idle);
            rwStatus = new AtomicObservable<NodeStatus>(NodeStatus.Idle);
            hints = new AtomicObservable<IEnumerable<INode>>(Enumerable.Empty<INode>());

            preJoinRw = new ObservableCondition(() => status.Get() == NodeStatus.Joining
                                                      && rwStatus.Get() == NodeStatus.Idle,
                                                new[] {status, rwStatus});
            preJoinRecon = new ObservableCondition(() => status.Get() == NodeStatus.Joining
                                                         && reconStatus.Get() == NodeStatus.Idle,
                                                   new[] {status, reconStatus});
            preJoin = new ObservableCondition(() => status.Get() == NodeStatus.Joining, new[] {status});
            preJoinAck = new ObservableCondition(() => status.Get() == NodeStatus.Joining
                                                       && rwStatus.Get() == NodeStatus.Active
                                                       && reconStatus.Get() == NodeStatus.Active,
                                                 new[] {status, rwStatus, reconStatus});

            this.recon = recon;
            this.recon.JoinAck += JoinAckReaderWriter;
            this.readerWriter = readerWriter;
            this.readerWriter.JoinAck += JoinAckRecon;

            new Thread(OutJoinRw).Start();
            new Thread(OutJoinRecon).Start();
            new Thread(OutSend).Start();
            new Thread(OutJoinAck).Start();
        }

        private void OutJoinAck()
        {
            preJoinAck.Waitable.WaitOne();
            status.Set(NodeStatus.Active);

            var handler = eventHandlers[JoinAckEvent] as RamboJoinAckEventHandler;
            if (handler != null)
            {
                handler();
            }
        }

        private void OutSend()
        {
            preJoin.Waitable.WaitOne();

            foreach (var node in hints.Get())
            {
                messageHub.Send(node, new JoinRwMessage(i));
            }
        }

        private void OutJoinRecon()
        {
            preJoinRecon.Waitable.WaitOne();
            recon.Join(i);
            reconStatus.Set(NodeStatus.Joining);
        }

        private void OutJoinRw()
        {
            preJoinRw.Waitable.WaitOne();
            readerWriter.Join(i);
            rwStatus.Set(NodeStatus.Joining);
        }

        private void JoinAckRecon(INode node)
        {
            preJoin.Waitable.WaitOne();
            reconStatus.Set(NodeStatus.Active);
        }

        private void JoinAckReaderWriter(INode node)
        {
            preJoin.Waitable.WaitOne();
            rwStatus.Set(NodeStatus.Active);
        }

        public void Join(IEnumerable<INode> initialWorld)
        {
            // TODO: Entry point.
            // Here all status handling threads (see ctor) might be started
            if (status.Get() == NodeStatus.Idle)
            {
                status.Set(NodeStatus.Joining);
                hints.Set(initialWorld);
            }
        }

        public void Fail()
        {
            status.Set(NodeStatus.Failed);
        }

        public event RamboJoinAckEventHandler JoinAck
        {
            add { eventHandlers.AddHandler(JoinAckEvent, value); }
            remove { eventHandlers.RemoveHandler(JoinAckEvent, value); }
        }
    }
}