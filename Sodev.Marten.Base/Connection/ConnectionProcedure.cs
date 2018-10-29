using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using Sodev.Marten.Base.Communication;

namespace Sodev.Marten.Base.Connection
{
    public class ConnectionProcedure
    {
        private readonly SerialPort port;

        private IList<ConnectionProcedureStep> procedureSteps;

        public ConnectionProcedure(SerialPort port)
        {
            this.port = port;

            ConfigureProcedureSteps();
        }

        private void ConfigureProcedureSteps()
        {
            procedureSteps = new List<ConnectionProcedureStep>
            {
                new ConnectionProcedureStep(StepName.ResetInterface, () => SendAtCommand("ATZ"), () => IsAnswerCorrect("ELM327")),
                new ConnectionProcedureStep(StepName.DisableEcho, () => SendAtCommand("ATE0"), () => IsAnswerCorrect("OK")),
                new ConnectionProcedureStep(StepName.SetAutoProtocol, () => SendAtCommand("ATSP0"), () => IsAnswerCorrect("OK")),
                new ConnectionProcedureStep(StepName.CheckCarConnection, () => SendAtCommand("0100"), VerifyCarConnection),
                new ConnectionProcedureStep(StepName.CheckProtocol, () => SendAtCommand("ATDP"), VerifyAndPublishNewProtocol),
                new ConnectionProcedureStep(StepName.DisableHeaders, () => SendAtCommand("ATH0"), () => IsAnswerCorrect("OK")),
                new ConnectionProcedureStep(StepName.EnableSeparators, () => SendAtCommand("ATS1"), () => IsAnswerCorrect("OK"))
            };
        }

        public async Task StartConnectionProcedure()
        {
            port.Open();

            foreach (var step in procedureSteps)
            {
                await Task.Delay(500);
                await PerformAStepAndVerifyResult(step);
            }
        }

        private async Task PerformAStepAndVerifyResult(ConnectionProcedureStep step)
        {
            var isAnswerCorrect = false;

            for (int i = 0; i < 5; i++)
            {
                step.StepAction.Invoke();
                await Task.Delay(2000);

                if (step.IsAnswerExpected)
                {
                    isAnswerCorrect = step.VerifyAnswerAction.Invoke();
                    if (isAnswerCorrect) break;
                }
                else
                {
                    break;
                }
            }

            //TODO change it to connection procedure exception:
            if (!isAnswerCorrect) throw new Exception($"Unexpected answer on step {step.StepName}");

        }

        private bool IsAnswerCorrect(string expectedAnswer)
        {
            var answer = port.ReadExisting();
            return answer.Contains(expectedAnswer); //TODO for now only if contains is checked.
        }

        private bool VerifyAndPublishNewProtocol()
        {
            var name = port.ReadExisting();
            //TODO here new protocol name has to be published...
            return true;
        }

        private bool VerifyCarConnection()
        {
            return true;
        }

        private void SendAtCommand(string command)
        {
            port.WriteLine($"{command}\r");
        }

        private enum StepName
        {
            NotStarted,
            ResetInterface,
            DisableEcho,
            SetAutoProtocol,
            CheckCarConnection,
            CheckProtocol,
            DisableHeaders,
            EnableSeparators
        }

        private class ConnectionProcedureStep
        {
            public ConnectionProcedureStep() {}
            public ConnectionProcedureStep(StepName stepName, Action stepAction, Func<bool> verifyAnswerAction =null)
            {
                StepName = stepName;
                StepAction = stepAction;
                VerifyAnswerAction = verifyAnswerAction;
            }

            public StepName StepName { get; set; }
            public Action StepAction { get; set; }
            public Func<bool> VerifyAnswerAction { get; set; }
            public bool IsAnswerExpected => VerifyAnswerAction != null;
        }
    }
}
