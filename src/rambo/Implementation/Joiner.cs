using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using rambo.Interfaces;
using rambo.Messaging;

namespace rambo.Implementation
{
    public class Joiner : IJoiner
    {
        private readonly IReaderWriter readerWriter;
        private readonly IRecon recon;
        private readonly IAtomicObservable<NodeStatus> status;
        private readonly IAtomicObservable<NodeStatus> reconStatus;
        private readonly IAtomicObservable<NodeStatus> rwStatus;
        private readonly IAtomicObservable<IEnumerable<INode>> hints;
        private readonly IObservableCondition preJoinRW;
        private readonly IObservableCondition preJoinRecon;
        private readonly IObservableCondition preJoin;
        private readonly INode i;
        private readonly IListener listener;

        public Joiner(INode i, IReaderWriter readerWriter, IRecon recon, IMessageHub messageHub)
        {
            this.i = i;
            status = new AtomicObservable<NodeStatus>(NodeStatus.Idle);
            reconStatus = new AtomicObservable<NodeStatus>(NodeStatus.Idle);
            rwStatus = new AtomicObservable<NodeStatus>(NodeStatus.Idle);
            hints = new AtomicObservable<IEnumerable<INode>>(Enumerable.Empty<INode>());

            preJoinRW = new ObservableCondition(() => status.Get() == NodeStatus.Joining && rwStatus.Get() == NodeStatus.Idle,
                                                new[] {status, rwStatus});
            preJoinRecon = new ObservableCondition(() => status.Get() == NodeStatus.Joining && reconStatus.Get() == NodeStatus.Idle,
                                                new[] {status, reconStatus});
            preJoin = new ObservableCondition(() => status.Get() == NodeStatus.Joining, new[] {status});

            this.recon = recon;
            this.recon.JoinAck += ReconJoinAck;
            this.readerWriter = readerWriter;
            this.readerWriter.JoinAck += ReaderWriterJoinAck;

            listener = messageHub.Subscribe(this.i);
            listener.Subscribe();
            new Thread(OutJoinRw).Start();
            new Thread(OutJoinRecon).Start();
        }

        private void OutJoinRecon()
        {
            while (true)
            {
                preJoinRecon.Waitable.WaitOne();
                recon.Join(i);
                reconStatus.Set(NodeStatus.Joining);
            }
        }

        private void OutJoinRw()
        {
            while (true)
            {
                preJoinRW.Waitable.WaitOne();
                readerWriter.Join(i);
                rwStatus.Set(NodeStatus.Joining);
            }
        }

        private void ReaderWriterJoinAck(INode node)
        {
        }

        private void ReconJoinAck(INode node)
        {
            throw new NotImplementedException();
        }

        public void Join(IEnumerable<INode> initialWorld)
        {
            if (status.Get() == NodeStatus.Idle)
            {
                status.Set(NodeStatus.Joining);
                hints.Set(initialWorld);
            }
        }

        public void Fail()
        {
            throw new System.NotImplementedException();
        }

        public event RamboJoinAckEventHandler JoinAck;
    }
}