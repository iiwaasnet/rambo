using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Linq;
using rambo.Interfaces;
using rambo.Messaging;

namespace rambo.Implementation
{
    public class Rambo : IRambo
    {
        private readonly IJoiner joiner;
        private readonly IReaderWriter readerWriter;
        private readonly IRecon recon;
        private readonly EventHandlerList eventHandlers;
        private readonly object ReadAckEvent = new object();
        private readonly object WriteAckEvent = new object();

        public Rambo(INode creator,
                     IMessageHub messageHub,
                     IEnumerable<IConfiguration> initialConfig,
                     IMessageSerializer messageSerializer)
        {
            Contract.Requires(creator != null);
            Contract.Requires(messageHub != null);
            Contract.Requires(messageHub != null);
            Contract.Requires(initialConfig != null && initialConfig.Any());

            Node = creator;
            eventHandlers = new EventHandlerList();
            readerWriter = new ReaderWriter(creator, messageHub, initialConfig, messageSerializer);
            recon = new Recon();

            joiner = new Joiner(creator, readerWriter, recon, messageHub);

            ConfigureService();
        }

        private void ConfigureService()
        {
            readerWriter.ReadAck += OnReadAck;
            readerWriter.WriteAck += OnWriteAck;
        }

        private void OnWriteAck(INode node)
        {
            var handler = eventHandlers[WriteAckEvent] as WriteAckEventHandler;

            if (handler != null)
            {
                handler(node);
            }
        }

        private void OnReadAck(IObjectId objectId, IObjectValue objectValue)
        {
            var handler = eventHandlers[ReadAckEvent] as ReadAckEventHandler;

            if (handler != null)
            {
                handler(objectId, objectValue);
            }
        }

        public void Join(IEnumerable<INode> initialWorld)
        {
            joiner.Join(initialWorld);
        }

        public void Read(IObjectId x)
        {
            readerWriter.Read(x);
        }

        public void Write(IObjectId x, IObjectValue v)
        {
            readerWriter.Write(x, v);
        }

        public void Reconfigure(IConfiguration @from, IConfiguration to)
        {
            throw new System.NotImplementedException();
        }

        public void Fail()
        {
            readerWriter.Fail();
        }

        public event ReadAckEventHandler ReadAck
        {
            add { eventHandlers.AddHandler(ReadAckEvent, value); }
            remove { eventHandlers.RemoveHandler(ReadAckEvent, value); }
        }

        public event WriteAckEventHandler WriteAck
        {
            add { eventHandlers.AddHandler(WriteAckEvent, value); }
            remove { eventHandlers.RemoveHandler(WriteAckEvent, value); }
        }

        public INode Node { get; private set; }
    }
}