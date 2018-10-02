using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sodev.Marten.Base.Connection
{
    internal sealed class Connection
    {
        private readonly SerialPort port;

        internal Connection()
        {
            port = new SerialPort();
        }

        internal void Open()
        {
            if (State != ConnectionState.Ready)
                throw new InvalidOperationException($"Connection couldn't be opened w/o passed parameters. Connection state: {State}");

            port.Open();
            State = ConnectionState.Opened;
        }

        internal void Close()
        {
            if (State != ConnectionState.Opened)
                throw new InvalidOperationException($"Connection couldn't be closed. Connection state: {State}");

            port.Close();
            State = ConnectionState.Closed;
        }

        internal void SetParameters(ConnectionParameters parameters)
        {
            if (State != ConnectionState.Closed)
                throw new InvalidOperationException($"Parameters can't be set when connections is not closed. Connection state: {State}");

            port.PortName = parameters.PortName;
            port.BaudRate = parameters.BaudRate;
            //TODO more to be added
            State = ConnectionState.Ready;
        }

        internal ConnectionState State { get; private set; }
    }
}
