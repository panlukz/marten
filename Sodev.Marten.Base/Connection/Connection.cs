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

            SendAtCommand(AtCommand.NoEcho, true);
            //TODO these to make a lot of problems :-(
            //SendAtCommand(AtCommand.Headers, false);
            //SendAtCommand(AtCommand.NoSeparators, true);
                
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

        private void SendAtCommand(AtCommand command, bool state)
        {
            //if(GetState() == ConnectionState.Opened) throw new NotImplementedException("Sending AT commands when the connection is opened is not supported yet."); 
            var strCommand = $"AT{(char)command}{(state ? 0 : 1)}\r";
            port.Write(strCommand);
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
