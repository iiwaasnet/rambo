using rambo.Interfaces;

namespace rambo.Implementation
{
    public class Recon : IRecon
    {
        public void Join(INode i)
        {
            throw new System.NotImplementedException();
        }

        public void Reconfigure(IConfiguration @from, IConfiguration to, INode i)
        {
            throw new System.NotImplementedException();
        }

        public void RequestConfiguration(IConfigurationIndex k)
        {
            throw new System.NotImplementedException();
        }

        public void Fail(INode i)
        {
            throw new System.NotImplementedException();
        }

        public event JoinAckEventHandler JoinAck;
        public event NewConfigEventHandler NewConfig;
        public event ReconAckEventHandler ReconAck;
        public event ReportEventHandler Report;
    }
}