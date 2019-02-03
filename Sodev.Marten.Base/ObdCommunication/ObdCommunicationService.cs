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

        public void CloseOnError()
        {
            connection.CloseOnError();
        }

        public void SendQuery(ObdQuery query)
        {
            //if (state != ConnectionState.Opened) throw new InvalidOperationException("Connection is not opened");

            connection.WriteData(query.SerializedQuery);
            Debug.WriteLine($"Data sent: {query.SerializedQuery}");
        }

        private void DataReceived(object sender, string answer)
        {
            answer = answer.Replace("\r\n", ">");
            //remove new line special characters
            //answer = answer.Replace("\n", "").Replace("\r", "");

            //If answer contains '<' sign, it means there is actually more than one answer
            //meaning each answer has to be handled separately
            var answersArray = answer.Split('>');

            Debug.WriteLine($"Data received: {string.Concat(answersArray)}");

            foreach (var ans in answersArray)
            {
                if (ans.Equals("NO DATA")
                    || string.IsNullOrWhiteSpace(ans)) continue; //TODO these two have to be handled separately... //throw new NotImplementedException(); //in case if it's not a response for a PID request


                switch (DetermineAnswerType(ans))
                {
                    case AnswerType.Pid:
                        PublishPidAnswer(ans);
                        break;
                    case AnswerType.Dtc:
                        PublishDtcAnswer(ans);
                        break;
                    case AnswerType.ClearDtc:
                        PublishClearDtcAnswer();
                        break;
                    default:
                        break;
                }
            }

        }

        private AnswerType DetermineAnswerType(string answer)
        {
            if(answer.Equals("OK")) return AnswerType.ClearDtc;

            var serviceNb = 0;
            var conversionResult = int.TryParse(answer[1].ToString(), out serviceNb);

            if(serviceNb == 1)
                return AnswerType.Pid;
            else if (serviceNb == 3)
                return AnswerType.Dtc;

            throw new Exception("Well, something went definitely wrong...");
        }

        private void PublishDtcAnswer(string answer)
        {
            var dtcAnswer = new DtcAnswer(answer);
            DtcAnswerReceivedEvent?.Invoke(this, dtcAnswer);
        }

        private void PublishPidAnswer(string answer)
        {
            if (!string.IsNullOrWhiteSpace(answer) && !answer.Equals("OK"))
            {
                try
                {
                    var obdAnswer = new PidAnswer(answer);

                    if (obdAnswer.Data.Length > 0) //TODO refactor...
                        PidAnswerReceivedEvent?.Invoke(this, obdAnswer);
                }
                catch (FormatException ex)
                {
                    //TODO log it
                    Debug.WriteLine($"DE3 _______ FormatException has been thrown during parsing obd answer: {ex.Message} ");
                }
            }
        }

        private void PublishClearDtcAnswer()
        {
            DtcClearedEvent?.Invoke(this, true);
        }

        public event EventHandler<PidAnswer> PidAnswerReceivedEvent;

        public event EventHandler<DtcAnswer> DtcAnswerReceivedEvent;
        public event EventHandler<bool> DtcClearedEvent;


        public event EventHandler<ConnectionProcedureStateChangedPayload> ConnectionStateChanged
        {
            add => connection.StateChanged += value;
            remove => connection.StateChanged -= value;
        }
    }

    public interface IObdCommuncation
    {
        void SendQuery(ObdQuery query);
        event EventHandler<PidAnswer> PidAnswerReceivedEvent;
        event EventHandler<DtcAnswer> DtcAnswerReceivedEvent;
        IList<string> GetAvailablePorts();
        ConnectionState ConnectionState { get; }
        void SetConnectionParameters(ConnectionParameters parameters);
        Task OpenAsync();
        void Close();
        void CloseOnError();
        event EventHandler<ConnectionProcedureStateChangedPayload> ConnectionStateChanged;
        event EventHandler<bool> DtcClearedEvent;
    }

    internal enum AnswerType
    {
        Pid,
        Dtc,
        ClearDtc
    }
}
