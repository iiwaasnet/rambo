using System.Collections.Generic;

namespace rambo.Interfaces
{
    public delegate void ReadAckEventHandler(IObjectId x, IObjectValue v);

    public delegate void WriteAckEventHandler(INode i);

    public interface IRambo
    {
        /// <summary>
        /// join(rambo, J)i, J a ﬁnite subset of I − {i}, i ∈ I, such that if i = i0 then J = ∅
        /// </summary>
        /// <param name="initialWorld"></param>
        void Join(IEnumerable<INode> initialWorld);

        /// <summary>
        /// i ∈ I
        /// </summary>
        /// <param name="x"></param>
        void Read(IObjectId x);

        /// <summary>
        /// write(v)i, v ∈ V , i ∈ I
        /// </summary>
        /// <param name="x"></param>
        /// <param name="v"></param>
        void Write(IObjectId x, IObjectValue v);

        /// <summary>
        /// recon(c, c0)i, c, c0 ∈ C, i ∈ members(c), i ∈ I
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        void Reconfigure(IConfiguration from, IConfiguration to);

        /// <summary>
        /// i ∈ I
        /// </summary>
        void Fail();

        event ReadAckEventHandler ReadAck;

        event WriteAckEventHandler WriteAck;
        INode Node { get; }
    }
}