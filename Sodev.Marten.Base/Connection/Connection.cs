using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sodev.Marten.Base.Connection
{
    public sealed class Connection
    {
        private readonly SerialPort port;

        public Connection()
        {
            port = new SerialPort();
        }

        public void Open()
        {
            if (State != ConnectionState.Ready)
                throw new InvalidOperationException($"Connection couldn't be opened w/o passed parameters. Connection state: {State}");

            port.Open();
            State = ConnectionState.Opened;

            port.DataReceived += new SerialDataReceivedEventHandler(DataReceived);
        }

        public void Close()
        {
            if (State != ConnectionState.Opened)
                throw new InvalidOperationException($"Connection couldn't be closed. Connection state: {State}");

            port.Close();
            State = ConnectionState.Closed;

            port.DataReceived -= new SerialDataReceivedEventHandler(DataReceived);
        }

        public void SetParameters(ConnectionParameters parameters)
        {
            if (State != ConnectionState.Closed)
                throw new InvalidOperationException($"Parameters can't be set when connections is not closed. Connection state: {State}");

            port.PortName = parameters.PortName;
            port.BaudRate = parameters.BaudRate;
            //TODO more to be added
            port.Parity = Parity.None; 
            port.StopBits = StopBits.One;
            port.Handshake = Handshake.None;
            port.DataBits = 8;


            State = ConnectionState.Ready;
        }

        public void SendQuery(Query query)
        {
            port.Write(query.QueryText);
            port.DiscardOutBuffer();
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
                AnswerReceivedEvent(this, new Answer() { AnswerText = answer });
        }

        public delegate void AnswerReceivedHandler(object sender, Answer answer);

        public event AnswerReceivedHandler AnswerReceivedEvent;

        public ConnectionState State { get; private set; }
    }
}
