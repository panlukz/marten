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

        public Connection(IDomainEventAggregator domainEventAggregator)
        {
            port = new SerialPort();
            this.domainEventAggregator = domainEventAggregator;
        }

        ~Connection()
        {
            if(GetState() == ConnectionState.Opened)
                Close();
        }

        public string DeviceName { get; private set; } = string.Empty;

        public string ProtocolName { get; private set; } = string.Empty;

        public async void OpenAsync()
        {
            if (GetState() != ConnectionState.Ready)
                throw new InvalidOperationException($"Connection couldn't be opened w/o passed parameters. Connection state: {GetState()}");
            
            port.Open(); //TODO handle exceptions here
            SetState(ConnectionState.Opened);

            SendAtCommand(AtCommand.Reset);
            SendAtCommand(AtCommand.NoEcho, true);

            SendAtCommand(AtCommand.SetAutoProtocol);

            Thread.Sleep(1000); //TODO lol only for testing...
            port.Write("0100\r");

            for (int i = 0; i < 2; i++)
            {
                SendAtCommand(AtCommand.CheckProtocol);
                await Task.Delay(500);
                var a = port.ReadExisting()
                    .Replace("\r", string.Empty)
                    .Replace("\n", string.Empty)
                    .Replace(">", string.Empty);
                if (!string.IsNullOrEmpty(a))
                {
                    ProtocolName = a;
                    break;
                    
                }
                else if(i == 1) //second trial
                {
                    throw new Exception("error");
                }
            }
            

            //TODO these two make a lot of problems :-(
            SendAtCommand(AtCommand.NoHeaders, true);
            SendAtCommand(AtCommand.NoSeparators, false);
                
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

        private void SendAtCommand(string command, bool? state=null) //TODO string??
        {
            //if(GetState() == ConnectionState.Opened) throw new NotImplementedException("Sending AT commands when the connection is opened is not supported yet."); 
            var parsedState = state.HasValue ? state.Value ? "0" : "1" : string.Empty;
            var strCommand = $"AT{command}{parsedState}\r";
            Thread.Sleep(1000); //TODO lol only for testing...
            port.Write(strCommand);
            Debug.WriteLine($"AT RESPONSE: {port.ReadExisting()}");

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
