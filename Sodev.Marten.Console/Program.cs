using Sodev.Marten.Base.Connection;
using Sodev.Marten.Base.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sodev.Marten.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            var conParams = new ConnectionParameters
            {
                BaudRate = 9600,
                PortName = "COM5"
            };
            var connection = new Connection(null); //TODO

            connection.SetParameters(conParams);

            connection.Open();
            System.Console.WriteLine($"CONNECTION STATE:{connection.GetState()}");

            var pidParams = new PidParameters()
            {
                ReturnedBytesNb = 2,
                Id = 0x0C,
                Formula = "((A * 256) + B) / 4",
                Unit = "rpm",
                Description = "RPM"
            };

            connection.AnswerReceivedEvent += new Connection.AnswerReceivedHandler(AnswerReceived);

            pid = Pid.Create(pidParams);
            connection.SendQuery(new Query { QueryText = "ATE0\r" });
            var query = new Query {QueryText = "010C\r" };

            while(true)
            {
                connection.SendQuery(query);
                Thread.Sleep(200);
            }

        }

        private static Pid pid;

        private static void AnswerReceived(object sender, Answer answer)
        {
            if (!answer.AnswerText.StartsWith("41")) return; //in case if it's not a response for a PID request


            var byteArray = answer.AnswerText.Split(' ').Skip(2).Select(x => Convert.ToByte($"0x{x}", 16)).ToArray<byte>();

            var decipheredValue = Math.Round(pid.GetValue(byteArray));

            System.Console.WriteLine($"RPM: {decipheredValue}");
            
        }
    }
}
