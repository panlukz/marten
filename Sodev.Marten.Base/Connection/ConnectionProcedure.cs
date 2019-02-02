using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Threading.Tasks;

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
                new ConnectionProcedureStep(StepName.ResetInterface, () => SendAtCommand("ATZ"), VerifyAndPublishNewDeviceName,
                                            "Reseting connected interface..."),
                new ConnectionProcedureStep(StepName.DisableEcho, () => SendAtCommand("ATE0"), () => IsAnswerCorrect("OK"),
                                            "Setting up the interface... Disabling echo..."),
                new ConnectionProcedureStep(StepName.SetAutoProtocol, () => SendAtCommand("ATSP0"), () => IsAnswerCorrect("OK"),
                                            "Setting up the interface... Reseting communication protocol..."),
                new ConnectionProcedureStep(StepName.CheckCarConnection, () => SendAtCommand("0100"), VerifyCarConnection,
                                            "Verifying the vehicle connection..."),
                new ConnectionProcedureStep(StepName.CheckProtocol, () => SendAtCommand("ATDP"), VerifyAndPublishNewProtocol,
                                            "Vehicle connected. Establishing communication protocol..."),
                new ConnectionProcedureStep(StepName.DisableHeaders, () => SendAtCommand("ATH0"), () => IsAnswerCorrect("OK"),
                                            "Setting up the communication. Disabling headers..."),
                new ConnectionProcedureStep(StepName.EnableSeparators, () => SendAtCommand("ATS1"), () => IsAnswerCorrect("OK"),
                                            "Setting up the communication. Enabling separators..."),
                new ConnectionProcedureStep(StepName.Connected, () => { }, () => true, 
                                            "Everything OK. Communication with the vehicle is established.")
            };
        }

        public async Task StartConnectionProcedure()
        {
            port.Open();

            foreach (var step in procedureSteps)
            {
                await PerformAStepAndVerifyResult(step);

                PublishConnectionProgress(step.StepName);
            }
        }

        private void PublishConnectionProgress(StepName stepName)
        {
            var stepDescription = procedureSteps.First(x => x.StepName == stepName).FriendlyDescription;
            var progress = CalculateProgress();
            var eventPayload = new ConnectionProcedureStateChangedPayload(stepDescription, progress);

            StateUpdated?.Invoke(this, eventPayload);

            int CalculateProgress()
            {
                var currentStep = procedureSteps.First(x => x.StepName == stepName);
                var indexOfCurrentStep = procedureSteps.IndexOf(currentStep);
                return (int) ((indexOfCurrentStep + 1) * 100.0 / procedureSteps.Count);
            }
        }


        private async Task PerformAStepAndVerifyResult(ConnectionProcedureStep step)
        {
            var isAnswerAsExpected = false;
            const int nbOfAttempts = 5;

            for (int i = 0; i < nbOfAttempts; i++)
            {
                step.StepAction.Invoke();
                await Task.Delay(step.WaitTimeMs);

                isAnswerAsExpected = step.HandleAnswerAction.Invoke();
                if (isAnswerAsExpected) break;
            }

            if (!isAnswerAsExpected) throw new ConnectionProcedureException(step.StepName);
        }

        private bool IsAnswerCorrect(string expectedAnswer)
        {
            var answer = port.ReadExisting();
            return answer.Contains(expectedAnswer); //TODO for now only if contains is checked.
        }

        private bool VerifyAndPublishNewDeviceName()
        {
            var deviceName = port.ReadExisting();
            DeviceNameUpdated?.Invoke(this, new ConnectionProcedureParameterUpdatedPayload("DeviceName", deviceName));
            return true; //TODO is it as expected??
        }

        private bool VerifyAndPublishNewProtocol()
        {
            var protocolName = port.ReadExisting();
            ProtocolNameUpdated?.Invoke(this, new ConnectionProcedureParameterUpdatedPayload("ProtocolName", protocolName));
            return true;
        }

        private bool VerifyCarConnection()
        {
            var carConnection = port.ReadExisting();
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
            EnableSeparators,
            Connected
        }

        private class ConnectionProcedureStep
        {
            public ConnectionProcedureStep() {}
            public ConnectionProcedureStep(StepName stepName, Action stepAction, Func<bool> handleAnswerAction=null, string description = null, int waitTimeMs=1000)
            {
                StepName = stepName;
                StepAction = stepAction;
                WaitTimeMs = waitTimeMs;
                HandleAnswerAction = handleAnswerAction;
                if (!string.IsNullOrWhiteSpace(description)) FriendlyDescription = description;
            }

            public StepName StepName { get; set; }
            public string FriendlyDescription { get; set; } = "Working hard...";
            public Action StepAction { get; set; }
            public int WaitTimeMs { get; set; }
            public Func<bool> HandleAnswerAction { get; set; }
        }

        private class ConnectionProcedureException : Exception
        {
            public ConnectionProcedureException(StepName step) : base($"Exception occured on executing step {step}")
            {}
        }

        public event EventHandler<ConnectionProcedureStateChangedPayload> StateUpdated;

        public event EventHandler<ConnectionProcedureParameterUpdatedPayload> ProtocolNameUpdated;

        public event EventHandler<ConnectionProcedureParameterUpdatedPayload> DeviceNameUpdated;

    }

    public class ConnectionProcedureStateChangedPayload
    {
        public ConnectionProcedureStateChangedPayload(string description, int progress)
        {
            Description = description;
            Progress = progress;
        }

        public string Description { get; }
        public int Progress { get; }
    }

    public class ConnectionProcedureParameterUpdatedPayload
    {
        public ConnectionProcedureParameterUpdatedPayload(string parameterName, string parameterValue)
        {
            ParameterName = parameterName;
            ParameterValue = parameterValue;
        }

        public string ParameterName { get; }
        public string ParameterValue { get; }
    }

}
