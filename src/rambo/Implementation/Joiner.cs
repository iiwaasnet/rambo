using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using rambo.Interfaces;

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
        private readonly INode i;

        public Joiner(INode i, IReaderWriter readerWriter, IRecon recon)
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

            this.recon = recon;
            this.recon.JoinAck += ReconJoinAck;
            this.readerWriter = readerWriter;
            this.readerWriter.JoinAck += ReaderWriterJoinAck;
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

        private void DoLocallyControlledActions()
        {
            while (true)
            {
                var index = WaitHandle.WaitAny(new[] {preJoinRW.Waitable, preJoinRecon.Waitable});

                switch (index)
                {
                    case WaitHandle.WaitTimeout:
                        break;
                    case 0:
                        readerWriter.Join(i);
                        rwStatus.Set(NodeStatus.Joining);
                        break;
                    case 1:
                        recon.Join(i);
                        reconStatus.Set(NodeStatus.Joining);
                        break;
                    default:
                        break;
                }
            }
        }

        public event RamboJoinAckEventHandler JoinAck;
    }
}