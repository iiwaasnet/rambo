using System.Collections.Generic;

namespace rambo.Interfaces
{
    public delegate void RamboJoinAckEventHandler();

    public interface IJoiner
    {
        void Join(IEnumerable<INode> initialWorld);

        void Fail();

        event RamboJoinAckEventHandler JoinAck;
    }
}