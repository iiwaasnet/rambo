using System;
using System.Collections.Generic;
using rambo.Interfaces;

namespace rambo.Implementation
{
    public class Joiner : IJoiner
    {
        private IReaderWriter readerWriter;
        private readonly IRecon recon;
        private NodeStatus status;

        public Joiner(IReaderWriter readerWriter, IRecon recon)
        {
            status = NodeStatus.Idle;
            this.recon = recon;
            this.recon.JoinAck += ReconJoinAck;
            this.readerWriter = readerWriter;
            this.readerWriter.JoinAck += ReaderWriterJoinAck;
        }

        private void ReaderWriterJoinAck(INode node)
        {
            new Precondition(() => { });
        }

        private void ReconJoinAck(INode node)
        {
            throw new NotImplementedException();
        }

        public void Join(IEnumerable<INode> initialWorld)
        {
            throw new System.NotImplementedException();
        }

        public void Fail()
        {
            throw new System.NotImplementedException();
        }

        public event RamboJoinAckEventHandler JoinAck;
    }
}