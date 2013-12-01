using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using rambo.Implementation;
using rambo.Interfaces;
using Topshelf;

namespace rambo
{
    public class RamboService : ServiceControl
    {
        private readonly IRamboFactory ramboFactory;
        private readonly IConfiguration initialConfig;
        private readonly ILogger logger;
        private IEnumerable<IRambo> farm;
        private readonly AutoResetEvent written;
        private readonly AutoResetEvent read;

        public RamboService(IRamboFactory ramboFactory, IConfiguration initialConfig, ILogger logger)
        {
            this.ramboFactory = ramboFactory;
            this.logger = logger;
            this.initialConfig = initialConfig;
            written = new AutoResetEvent(false);
            read = new AutoResetEvent(false);
        }

        public bool Start(HostControl hostControl)
        {
            try
            {
                farm = initialConfig.Nodes.Select(node => ramboFactory.Build(node)).ToArray();
                var initialWorld = Enumerable.Empty<INode>();

                foreach (var rambo in farm)
                {
                    // NOTE: if i = i0 then J = ∅
                    rambo.Join(initialWorld);
                    rambo.WriteAck += OnWriteAck;
                    rambo.ReadAck += OnReadAck;

                    initialWorld = initialWorld.Concat(new[] {rambo.Node});
                }

                new Thread(WorkLoop).Start();

                return true;
            }
            catch (Exception err)
            {
                logger.Error(err);

                return false;
            }
        }

        private void WorkLoop()
        {
            while (true)
            {
                var rambo = farm.First();

                rambo.Write(null, new ObjectValue {Value = 12});
                written.WaitOne();

                rambo.Read(null);
                read.WaitOne();
            }
        }

        private void OnWriteAck(INode node)
        {
            logger.Info("Value written");
            written.Set();
        }

        private void OnReadAck(IObjectId objectId, IObjectValue objectValue)
        {
            logger.InfoFormat("Read value: {0}", objectValue.Value);
            read.Set();
        }

        public bool Stop(HostControl hostControl)
        {
            return true;
        }
    }
}