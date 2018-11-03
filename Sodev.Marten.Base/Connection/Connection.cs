using Sodev.Marten.Base.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Threading.Tasks;

namespace Sodev.Marten.Base.Connection
{
    public class Connection : IConnection, IConnectionInfo
    {
        private SerialPort port;
        private ConnectionState state;
        private ConnectionProcedure connectionProcedure;

        public Connection()
        {
            port = new SerialPort();
            connectionProcedure = new ConnectionProcedure(port);
        }

        ~Connection()
        {
            if(GetState() == ConnectionState.Opened)
                Close();
        }

        public async Task OpenAsync()
        {
            if (GetState() != ConnectionState.Ready)
                throw new InvalidOperationException($"Connection couldn't be opened w/o passed parameters. Connection state: {GetState()}");

            await connectionProcedure.StartConnectionProcedure();

            SetState(ConnectionState.Opened);

            //connection procedure has finished so we can hook up the communication
            port.DataReceived += OnPortDataReceived;
        }

        private void OnPortDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            var data = port.ReadExisting();
            DataReceived?.Invoke(this, data);
        }

        public event EventHandler<string> DataReceived;

        public void WriteData(string data)
        {
            port.WriteLine($"{data}\r");
        }

        public void Close()
        {
            if (GetState() != ConnectionState.Opened)
                throw new InvalidOperationException($"Connection couldn't be closed. Connection state: {GetState()}");

            port.Close();
            SetState(ConnectionState.Closed);

            port.DataReceived -= OnPortDataReceived;
        }

        public void SetParameters(ConnectionParameters parameters)
        {
            if (GetState() != ConnectionState.Closed)
                throw new InvalidOperationException($"Parameters can't be set when connections is not closed. Connection state: {GetState()}");

            port.PortName = parameters.PortName;
            port.BaudRate = parameters.BaudRate;
            
            //the rest parameters have to be always the same
            port.Parity = Parity.None; 
            port.StopBits = StopBits.One;
            port.Handshake = Handshake.None;
            port.DataBits = 8;

            SetState(ConnectionState.Ready);
        }

        public ConnectionState GetState() => state;

        private void SetState(ConnectionState newState)
        {
            state = newState;
            //obdEventBus.PublishEvent(new ConnectionStateChanged(newState));
        }

        public IList<string> GetAvailablePorts() => SerialPort.GetPortNames().ToList();
    }
}
