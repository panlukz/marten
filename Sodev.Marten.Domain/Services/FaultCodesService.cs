using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Threading;
using System.Threading.Tasks;
using Sodev.Marten.Base.Connection;
using Sodev.Marten.Base.Model;
using Sodev.Marten.Base.ObdCommunication;
using Sodev.Marten.Domain.Events;
using Sodev.Marten.Domain.Helpers;

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

    public class Dtc //probably a base layer object
    {
        public string Id { get; private set; }

        public string DisplayId
        {
            get
            {
                var typeSign = DtcSubSystem.GetTypeDescription(int.Parse(Id[0].ToString()))[0];
                return typeSign + Id.Substring(1);
            }
        }

        public string Type => DtcSubSystem.GetTypeDescription(int.Parse(Id[0].ToString()));

        public string SubSystem => DtcSubSystem.GetSubsystemDescription(int.Parse(Id[2].ToString()));

        public string Desc { get; set; }
        
        private Dtc()
        {
        }

        public static List<Dtc> CreateList(byte[] data)
        {
            var list = new List<Dtc>();
            for (var i = 0; i < data.Length; i+=4)
            {
                var first = data[0+i] >> 2;
                var second = data[0+i] & 3;

                if(first == 0 && second == 0 && data[1 + i] == 0 && data[2 + i] == 0 && data[3 + i] == 0) 
                    break; //all codes read

                var specificCode = $"{data[1+i]:D}{data[2+i]:D}{data[3+i]:D}";
                var dtc = new Dtc();
                dtc.Id = $"{first}{second}{specificCode}";

                if (dtcDetails == null) dtcDetails = InitializeKnowDtcList();

                dtc.Desc = dtcDetails.FirstOrDefault(x => x.Id == dtc.Id)?.Desc;

                list.Add(dtc);
            }

            return list;
        }

        private static List<DtcDetails> dtcDetails;

        private static List<DtcDetails> InitializeKnowDtcList()
        {

            var hardcodedPath = "dtcdb.xml";//TODO get rid of it!
            var xmlString = File.ReadAllText(hardcodedPath);
            return XmlSerializeHelper.DeSerializeObject<List<DtcDetails>>(xmlString);
        }
    }

    public class DtcDetails
    {
        public string Id { get; set; }
        public string Desc { get; set; }
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

        public static string GetTypeDescription(int typeId)
        {
            if (!typeDesc.ContainsKey(typeId)) return string.Empty;

            return typeDesc[typeId];
        }

        public static string GetSubsystemDescription(int subsystemId)
        {
            if (!subsystemDesc.ContainsKey(subsystemId)) return string.Empty;

            return subsystemDesc[subsystemId];
        }
    }

}
