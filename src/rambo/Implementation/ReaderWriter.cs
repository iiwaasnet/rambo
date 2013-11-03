using System.ComponentModel;
using rambo.Interfaces;

namespace rambo.Implementation
{
    public class ReaderWriter : IReaderWriter
    {
        private readonly EventHandlerList eventHandlers;
        private readonly object ReadAckEvent = new object();
        private readonly object WriteAckEvent = new object();

        public ReaderWriter()
        {
            eventHandlers = new EventHandlerList();
        }

        public void Read(IObjectId x)
        {
            throw new System.NotImplementedException();
        }

        public void Write(IObjectId x, IObjectValue v)
        {
            throw new System.NotImplementedException();
        }

        public void Fail()
        {
            throw new System.NotImplementedException();
        }

        public void Join()
        {
            throw new System.NotImplementedException();
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
    }
}