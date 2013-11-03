namespace rambo.Interfaces
{
    public interface IReaderWriter
    {
        void Read(IObjectId x);

        void Write(IObjectId x, IObjectValue v);

        void Fail();

        void Join();

        //void NewConfig(IConfiguration c, IConfigurationIndex k);

        //void ReceiveJoin(INode j);

        //void ReceiveMessage(INode j, IMessage m);

        event ReadAckEventHandler ReadAck;
        event WriteAckEventHandler WriteAck;
        event JoinAckEventHandler JoinAck;
    }
}