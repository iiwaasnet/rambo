using System.Collections.Generic;

namespace rambo.Interfaces
{
    public delegate void ReadAckEventHandler(IObjectId x, IObjectValue v);

    public delegate void WriteAckEventHandler(INode i);

    public interface IRambo
    {
        void Join(IEnumerable<INode> initialWorld);

        void Read(IObjectId x);

        void Write(IObjectId x, IObjectValue v);

        void Reconfigure(IConfiguration from, IConfiguration to);

        void Fail();

        event ReadAckEventHandler ReadAck;

        event WriteAckEventHandler WriteAck;
        INode Node { get; }
    }
}