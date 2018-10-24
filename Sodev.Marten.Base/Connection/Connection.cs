using Sodev.Marten.Base.Events;
using Sodev.Marten.Base.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            
            port.Open(); //TODO handle exceptions here
            SetState(ConnectionState.Opened);

            SendQuery(new ObdQuery("ATE0\r"));

            port.DataReceived += DataReceived;
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

        public void SendQuery(ObdQuery query)
        {
            if(state != ConnectionState.Opened) throw new InvalidOperationException("Connection is not opened");

            port.Write($"{query.SerializedQuery}\r");
            //port.DiscardOutBuffer(); //TODO find out if it's necessary here??
        }

        private void DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            var answer = port.ReadExisting();
            //TODO lol this is definetly not very efficient... use regex or something
            answer = answer.Replace("\n", "");
            answer = answer.Replace("\r", "");

            //If answer contains < sign, it means there is actually more than one answer
            //meaning each answer has to be handled separately
            var answersArray = answer.Split('>');

            Debug.WriteLine($"DataReceived payload: {answer}");

            foreach (var ans in answersArray)
            {
                if (!ans.StartsWith("41")
                    && !string.IsNullOrEmpty(ans)
                    && !ans.Equals("OK")
                    && !ans.Equals(">")
                    && !ans.Equals("NO DATA")
                    && !ans.Equals("ATE0OK")) throw new NotImplementedException(); //in case if it's not a response for a PID request

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
