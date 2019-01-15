using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Policy;
using System.Threading;
using System.Threading.Tasks;
using Sodev.Marten.Base.Connection;
using Sodev.Marten.Base.ObdCommunication;
using Sodev.Marten.Domain.Events;

namespace Sodev.Marten.Domain.Services
{
    public interface IFaultCodesService
    {
        void RequestFaultCodes();
        void RequestClearingFaultCodes();
        int FaultCodesNumber { get; }
        bool FaultCodesPresent { get; }
        IList<Dtc> FaultCodesList { get; }
    }

    public class FaultCodesService : IFaultCodesService
    {
        private readonly IObdCommuncation obdCommuncation;
        private readonly IObdEventBus obdEventBus;
        private readonly ObdQuery getNumberOfStoredDtcQuery = new ObdQuery(1, 1);
        private readonly ObdQuery getStoredDtcQuery = new ObdQuery(3);
        private readonly ObdQuery clearStoredDtcQuery = new ObdQuery(4);
        private static ManualResetEvent mre = new ManualResetEvent(false);


        public FaultCodesService(IObdCommuncation obdCommuncation, IObdEventBus obdEventBus)
        {
            this.obdCommuncation = obdCommuncation;
            this.obdEventBus = obdEventBus;

            SubscribeAnswerReceivedEvent();
        }

        private void SubscribeAnswerReceivedEvent()
        {
            obdCommuncation.DtcAnswerReceivedEvent -= OnDtcAnswerReceived; //to ensure it won't be hooked up twice
            obdCommuncation.DtcAnswerReceivedEvent += OnDtcAnswerReceived;

            obdCommuncation.PidAnswerReceivedEvent -= OnPidAnswerReceived;
            obdCommuncation.PidAnswerReceivedEvent += OnPidAnswerReceived;

            obdCommuncation.DtcClearedEvent -= OnDtcClearedEvent;
            obdCommuncation.DtcClearedEvent += OnDtcClearedEvent;
        }



        //TODO use when leaving the view...
        private void UnsubscribeAnswerReceivedEvent()
        {
            obdCommuncation.DtcAnswerReceivedEvent -= OnDtcAnswerReceived;
            obdCommuncation.PidAnswerReceivedEvent -= OnPidAnswerReceived;
            obdCommuncation.DtcClearedEvent -= OnDtcClearedEvent;

        }

        private void OnPidAnswerReceived(object sender, PidAnswer e)
        {
            if (e.ServiceNb != 1 && e.PidId != 1) throw new Exception("Unexpected answer on this feature");

            var bitArray = new BitArray(new byte[] { e.Data[0] });

            var faultCodesPresent = bitArray.Get(0);
            bitArray.Set(7, false);

            var array = new int[1];
            bitArray.CopyTo(array, 0);

            FaultCodesNumber = array[0];
            Debug.WriteLine($"Retrieved number of fault codes {FaultCodesNumber}");

            obdEventBus.PublishEvent(new FaultCodeEvent(FaultCodeEventType.FaultCodesNumberUpdated));
            mre.Set();
        }

        private void OnDtcClearedEvent(object sender, bool e)
        {
            obdEventBus.PublishEvent(new FaultCodeEvent(FaultCodeEventType.FaultCodesCleared));
        }


        private void OnDtcAnswerReceived(object sender, DtcAnswer e)
        {
            FaultCodesList.Add(new Dtc(e.DtcNumber));
            obdEventBus.PublishEvent(new FaultCodeEvent(FaultCodeEventType.FaultCodesRetrieved));
        }

        public void RequestFaultCodes()
        {
            Task.Factory.StartNew( () =>
            {
                FaultCodesList.Clear();
                RequestStoredDtcNumber();
                mre.WaitOne();
                obdCommuncation.SendQuery(getStoredDtcQuery);
                mre.Reset();
            });
        }

        private void RequestStoredDtcNumber()
        {
            obdCommuncation.SendQuery(getNumberOfStoredDtcQuery);
        }

        public void RequestClearingFaultCodes()
        {
            obdCommuncation.SendQuery(clearStoredDtcQuery);
        }

        public int FaultCodesNumber { get; private set; }
        public bool FaultCodesPresent => FaultCodesNumber > 0;

        public IList<Dtc> FaultCodesList { get; private set; } = new List<Dtc>();

    }

    public class Dtc //probably a base layer object
    {
        public string Number { get; }

        public Dtc(string strDtc)
        {
            Number = strDtc;
        }
    }

    public enum DtcType
    {
        PowerTrain,
        Chassis,
        Body,
        Network
    }

    public enum DtcSpecificType
    {
        ObdSpecific,
        ManufacturerSpecific
    }

    public class DtcSubSystem
    {
        private static readonly Dictionary<int, string> subsystemDesc = new Dictionary<int, string>
        {
            {0, "Fuel and air metering and auxiliary emission controls"},
            {1, "Fuel and air metering"},
            {2, "Fuel and air metering (injector circuit)"},
            {3, "Ignition system or misfire"},
            {4, "Auxiliary emissions controls"},
            {5, "Vehicle speed controls and idle control system"},
            {6, "Computer output circuit"},
            {7, "Transmission"},
            {8, "Transmission"}
        };

        private static readonly Dictionary<int, string> typeDesc = new Dictionary<int, string>
        {
            {0, "Power train"},
            {1, "Chassis"},
            {2, "Body"},
            {3, "Network"}
        };

        public string GetTypeDescription(int typeId)
        {
            if (!subsystemDesc.ContainsKey(typeId)) return string.Empty;

            return subsystemDesc[typeId];
        }

        public string GetSubsystemDescription(int subsystemId)
        {
            if (!subsystemDesc.ContainsKey(subsystemId)) return string.Empty;

            return subsystemDesc[subsystemId];
        }
    }

}
