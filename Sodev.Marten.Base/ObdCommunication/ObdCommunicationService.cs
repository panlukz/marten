using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using Sodev.Marten.Base.Connection;
using Sodev.Marten.Base.Services;

namespace Sodev.Marten.Base.ObdCommunication
{

    public class ObdSerialCommunication : IObdCommuncation
    {
        private readonly IConnection connection;

        public ObdSerialCommunication()
        {
            connection = new Connection.Connection();
            connection.DataReceived += DataReceived;
        }

        public ConnectionState ConnectionState => connection.GetState();

        public IList<string> GetAvailablePorts() => connection.GetAvailablePorts();

        public void SetConnectionParameters(ConnectionParameters parameters)
        {
            connection.SetParameters(parameters);
        }

        public async Task OpenAsync()
        {
            await connection.OpenAsync();
        }

        public void Close()
        {
            connection.Close();
        }

        public void SendQuery(ObdQuery query)
        {
            //if (state != ConnectionState.Opened) throw new InvalidOperationException("Connection is not opened");

            connection.WriteData(query.SerializedQuery);
            Debug.WriteLine($"Data sent: {query.SerializedQuery}");
        }

        private void DataReceived(object sender, string answer)
        {
            //remove new line special characters
            answer = answer.Replace("\n", "").Replace("\r", "");

            //If answer contains '<' sign, it means there is actually more than one answer
            //meaning each answer has to be handled separately
            var answersArray = answer.Split('>');

            Debug.WriteLine($"Data received: {string.Concat(answersArray)}");

            foreach (var ans in answersArray)
            {
                if (ans.Equals("OK")
                    || ans.Equals("NO DATA")) continue; //these two have to be handled separately... //throw new NotImplementedException(); //in case if it's not a response for a PID request

                PublishObdAnswer(ans);
            }
        }

        private void PublishObdAnswer(string answer)
        {
            if (!string.IsNullOrWhiteSpace(answer) && !answer.Equals("OK"))
            {
                try
                {
                    var obdAnswer = new ObdAnswer(answer);

                    if (obdAnswer.Data.Length > 0) //TODO refactor...
                        AnswerReceivedEvent?.Invoke(this, obdAnswer);
                }
                catch (FormatException ex)
                {
                    //TODO log it
                    Debug.WriteLine($"DE3 _______ FormatException has been thrown during parsing obd answer: {ex.Message} ");
                }
            }
        }

        public event EventHandler<ObdAnswer> AnswerReceivedEvent;

        public event EventHandler<ConnectionProcedureStateChangedPayload> ConnectionStateChanged
        {
            add => connection.StateChanged += value;
            remove => connection.StateChanged -= value;
        }
    }

    public interface IObdCommuncation
    {
        void SendQuery(ObdQuery query);
        event EventHandler<ObdAnswer> AnswerReceivedEvent;
        IList<string> GetAvailablePorts();
        ConnectionState ConnectionState { get; }
        void SetConnectionParameters(ConnectionParameters parameters);
        Task OpenAsync();
        void Close();
        event EventHandler<ConnectionProcedureStateChangedPayload> ConnectionStateChanged;
    }
}
