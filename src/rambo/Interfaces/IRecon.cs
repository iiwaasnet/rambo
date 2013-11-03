namespace rambo.Interfaces
{
    public delegate void JoinAckEventHandler(INode i);
    public delegate void ReconAckEventHandler(INode i);
    public delegate void NewConfigEventHandler(IConfiguration c, IConfigurationIndex k, INode i);
    public delegate void ReportEventHandler(IConfiguration c, INode i);

    public interface IRecon
    {
        void Join(INode i);

        void Reconfigure(IConfiguration from, IConfiguration to, INode i);

        void RequestConfiguration(IConfigurationIndex k);

        void Fail(INode i);

        event JoinAckEventHandler JoinAck;

        event NewConfigEventHandler NewConfig;

        event ReconAckEventHandler ReconAck;

        event ReportEventHandler Report;
    }
}