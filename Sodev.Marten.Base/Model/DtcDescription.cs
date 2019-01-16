using System.Collections.Generic;
using System.IO;
using Sodev.Marten.Base.Helpers;

namespace Sodev.Marten.Base.Model
{
    public class DtcDescription
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

        public static List<DtcDetails> InitializeKnowDtcList()
        {

            var hardcodedPath = "dtcdb.xml";//TODO get rid of it!
            var xmlString = File.ReadAllText(hardcodedPath);
            return XmlSerializeHelper.DeSerializeObject<List<DtcDetails>>(xmlString);
        }
    }
}