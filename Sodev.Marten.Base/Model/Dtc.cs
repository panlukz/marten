using System.Collections.Generic;
using System.Linq;

namespace Sodev.Marten.Base.Model
{
    public class Dtc //probably a base layer object
    {
        public string Id { get; private set; }

        public string DisplayId
        {
            get
            {
                var typeSign = DtcDescription.GetTypeDescription(int.Parse(Id[0].ToString()))[0];
                return typeSign + Id.Substring(1);
            }
        }

        public string Type => DtcDescription.GetTypeDescription(int.Parse(Id[0].ToString()));

        public string SubSystem => DtcDescription.GetSubsystemDescription(int.Parse(Id[2].ToString()));

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

                if (dtcDetails == null) dtcDetails = DtcDescription.InitializeKnowDtcList();

                dtc.Desc = dtcDetails.FirstOrDefault(x => x.Id == dtc.Id)?.Desc;

                list.Add(dtc);
            }

            return list;
        }

        private static List<DtcDetails> dtcDetails;

        
    }
}