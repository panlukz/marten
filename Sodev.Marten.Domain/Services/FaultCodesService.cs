using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Policy;
using System.Threading;
using System.Threading.Tasks;
using Sodev.Marten.Base.Connection;
using Sodev.Marten.Base.Model;
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
        List<Dtc> FaultCodesList { get; }
        void UnsubscribeAnswerReceivedEvent();
        void SubscribeAnswerReceivedEvent();
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
        }

        public void SubscribeAnswerReceivedEvent()
        {
            obdCommuncation.DtcAnswerReceivedEvent -= OnDtcAnswerReceived; //to ensure it won't be hooked up twice
            obdCommuncation.DtcAnswerReceivedEvent += OnDtcAnswerReceived;

            obdCommuncation.PidAnswerReceivedEvent -= OnPidAnswerReceived;
            obdCommuncation.PidAnswerReceivedEvent += OnPidAnswerReceived;

            obdCommuncation.DtcClearedEvent -= OnDtcClearedEvent;
            obdCommuncation.DtcClearedEvent += OnDtcClearedEvent;
        }



        public void UnsubscribeAnswerReceivedEvent()
        {
            obdCommuncation.DtcAnswerReceivedEvent -= OnDtcAnswerReceived;
            obdCommuncation.PidAnswerReceivedEvent -= OnPidAnswerReceived;
            obdCommuncation.DtcClearedEvent -= OnDtcClearedEvent;

        }

        private void OnPidAnswerReceived(object sender, PidAnswer e)
        {
            if (e.ServiceNb != 1 && e.PidId != 1) throw new Exception("Unexpected answer on this feature");

            var bitArray = new BitArray(new byte[] {e.Data[0]});

            var isMilOn = bitArray.Get(0);
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
            Task.Factory.StartNew(RetrieveFaultCodes)
                .ContinueWith(t =>
                    obdEventBus.PublishEvent(new FaultCodeEvent(FaultCodeEventType.FaultCodesCleared)));
        }

        private void OnDtcAnswerReceived(object sender, DtcAnswer e)
        {
            FaultCodesList.AddRange(Dtc.CreateList(e.Data));
            obdEventBus.PublishEvent(new FaultCodeEvent(FaultCodeEventType.FaultCodesRetrieved));
        }

        public void RequestFaultCodes()
        {
            Task.Factory.StartNew(RetrieveFaultCodes);
        }

        private void RetrieveFaultCodes()
        {
            FaultCodesList.Clear();
            RequestStoredDtcNumber();
            mre.WaitOne();
            obdCommuncation.SendQuery(getStoredDtcQuery);
            mre.Reset();
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

        public List<Dtc> FaultCodesList { get; private set; } = new List<Dtc>();

}
}
