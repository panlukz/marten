using Sodev.Marten.Base.Events;
using Sodev.Marten.Base.Services;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sodev.Marten.Base.Connection
{
    public class Connection : IConnectionService, IConnectionInfo
    {
        private readonly SerialPort port;
        private readonly IDomainEventAggregator domainEventAggregator;
        private ConnectionState state;

        public Connection(IDomainEventAggregator domainEventAggregator)
        {
            port = new SerialPort();
            this.domainEventAggregator = domainEventAggregator;
        }

        public void Open()
        {
            if (GetState() != ConnectionState.Ready)
                throw new InvalidOperationException($"Connection couldn't be opened w/o passed parameters. Connection state: {GetState()}");
            
            port.Open();
            SetState(ConnectionState.Opened);

            SendQuery(new Query { QueryText = "ATE0\r" });

            port.DataReceived += new SerialDataReceivedEventHandler(DataReceived);
        }

        public void Close()
        {
            if (GetState() != ConnectionState.Opened)
                throw new InvalidOperationException($"Connection couldn't be closed. Connection state: {GetState()}");

            port.Close();
            SetState(ConnectionState.Closed);

            port.DataReceived -= new SerialDataReceivedEventHandler(DataReceived);
        }

        public void SetParameters(ConnectionParameters parameters)
        {
            if (GetState() != ConnectionState.Closed)
                throw new InvalidOperationException($"Parameters can't be set when connections is not closed. Connection state: {GetState()}");

            port.PortName = parameters.PortName;
            port.BaudRate = parameters.BaudRate;
            //TODO more to be added
            port.Parity = Parity.None; 
            port.StopBits = StopBits.One;
            port.Handshake = Handshake.None;
            port.DataBits = 8;

            SetState(ConnectionState.Ready);
        }

        public void SendQuery(Query query)
        {
            if(state != ConnectionState.Opened) return;

            port.Write($"{query.QueryText}\r");
            port.DiscardOutBuffer(); //TODO find out if it's necessary here??
        }

        private void DataReceived(object sender, SerialDataReceivedEventArgs e)
        { 
            var answer = port.ReadExisting();
            //TODO lol this is definetly not very efficient... use regex or something
            answer = answer.Replace("\n", "");
            answer = answer.Replace("\r", "");
            //answer = answer.Replace(" ", "");
            answer = answer.Replace(">", "");

            if (!string.IsNullOrWhiteSpace(answer))
                AnswerReceivedEvent?.Invoke(this, new Answer() { AnswerText = answer });
        }

        public delegate void AnswerReceivedHandler(object sender, Answer answer);

        public event AnswerReceivedHandler AnswerReceivedEvent;

        public ConnectionState GetState() => state;

        private void SetState(ConnectionState newState)
        {
            state = newState;
            domainEventAggregator.PublishDomainEvent(new ConnectionStateChanged(newState));
        }

        public IList<string> GetAvailablePorts() => SerialPort.GetPortNames().ToList();
    }
}
