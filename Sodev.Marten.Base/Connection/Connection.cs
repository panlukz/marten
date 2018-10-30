using Sodev.Marten.Base.Events;
using Sodev.Marten.Base.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sodev.Marten.Base.Connection
{
    public class Connection : IConnectionService, IConnectionInfo
    {
        private readonly SerialPort port;
        private readonly IDomainEventAggregator domainEventAggregator;
        private ConnectionState state;
        private ConnectionProcedure connectionProcedure;

        public Connection(IDomainEventAggregator domainEventAggregator)
        {
            port = new SerialPort();
            connectionProcedure = new ConnectionProcedure(port);
            this.domainEventAggregator = domainEventAggregator;
        }

        ~Connection()
        {
            if(GetState() == ConnectionState.Opened)
                Close();
        }

        public string DeviceName { get; private set; } = string.Empty;

        public string ProtocolName { get; private set; } = string.Empty;

        public async Task OpenAsync()
        {
            if (GetState() != ConnectionState.Ready)
                throw new InvalidOperationException($"Connection couldn't be opened w/o passed parameters. Connection state: {GetState()}");

            await connectionProcedure.StartConnectionProcedure();

            SetState(ConnectionState.Opened);

            //TODO for some reason it's important to subscribe this method after AT commands are sent. find out why?
            port.DataReceived += DataReceived;
        }

        public void Close()
        {
            if (GetState() != ConnectionState.Opened)
                throw new InvalidOperationException($"Connection couldn't be closed. Connection state: {GetState()}");

            port.Close();
            SetState(ConnectionState.Closed);

            port.DataReceived -= DataReceived;
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

        public void SendQuery(ObdQuery query)
        {
            if(state != ConnectionState.Opened) throw new InvalidOperationException("Connection is not opened");

            port.Write($"{query.SerializedQuery}\r");
            Debug.WriteLine($"Data sent: {query.SerializedQuery}");
        }

        private void WriteToPort(string message)
        {
            //Task.Factory.StartNew()
        }

        private void DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            var answer = port.ReadExisting();

            //remove new line special characters
            answer = answer.Replace("\n", "").Replace("\r", "");

            //If answer contains '<' sign, it means there is actually more than one answer
            //meaning each answer has to be handled separately
            var answersArray = answer.Split('>');

            Debug.WriteLine($"Data received: {string.Concat(answersArray)}");

            foreach (var ans in answersArray)
            {
                if (!ans.StartsWith("41")
                    && !string.IsNullOrEmpty(ans)
                    && !ans.Equals("OK")
                    && !ans.Equals(">")
                    && !ans.Equals("NO DATA")
                    && !ans.Equals("ATE0OK")) return;//throw new NotImplementedException(); //in case if it's not a response for a PID request

                PublishObdAnswer(ans);
            }
        }

        private void PublishObdAnswer(string answer)
        {
            if (!string.IsNullOrWhiteSpace(answer) && !answer.Equals("OK"))
                AnswerReceivedEvent?.Invoke(this, new ObdAnswer(answer));
        }

        public delegate void AnswerReceivedHandler(object sender, ObdAnswer answer);

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
