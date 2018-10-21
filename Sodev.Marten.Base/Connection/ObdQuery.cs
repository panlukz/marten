using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sodev.Marten.Base.Connection
{
    public class ObdQuery
    {
        public string QueryText { get; set; }
    }

    public class ObdAnswer
    {
        public ObdAnswer(string answerText)
        {
            Mode =  Convert.ToInt32(answerText.Substring(1, 1));
            PidId = Convert.ToInt32(answerText.Substring(3, 2), 16);
            Data = answerText.Substring(6, answerText.Length - 6).Split(' ').Select(x => Convert.ToByte(x, 16)).ToArray();
        }

        public int Mode { get; private set; }

        public int PidId { get; private set; }

        public byte[] Data { get; private set; }

        public DateTime TimeStamp { get; private set; } = DateTime.Now;
    }
}
