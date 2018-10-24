using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Sodev.Marten.Base.Connection
{
    public class ObdQuery
    {
        [Obsolete("This method will be removed soon")]
        public ObdQuery(string queryString)
        {
            SerializedQuery = queryString;
        }

        public ObdQuery(int serviceNb)
        {
            SerializedQuery = $"{serviceNb:D2}";
        }

        public ObdQuery(int serviceNb, int pidId)
        {
            SerializedQuery = $"{serviceNb:D2}{pidId:X2}";
        }

        public string SerializedQuery { get; }
    }
}
