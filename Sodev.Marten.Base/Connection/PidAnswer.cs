using System;
using System.Globalization;
using System.Linq;
using Sodev.Marten.Base.ExceptionHandling;

namespace Sodev.Marten.Base.Connection
{
    public abstract class ObdAnswer
    {
        protected ObdAnswer(string answerText)
        {
            ServiceNb = DeserializeServiceNb(answerText.Substring(1, 1));
        }

        public int ServiceNb { get; private set; }

        private int DeserializeServiceNb(string serviceStr)
        {
            int serviceNb = Convert.ToInt32(serviceStr); //throws FormatException when conversion fails
            if (serviceNb < 1 || serviceNb > 9) throw new FormatException($"Service should be a number within 1-9 range. It was: {serviceNb}");

            return serviceNb;
        }

    }

    public class PidAnswer : ObdAnswer
    {
        public PidAnswer(string answerText) : base(answerText)
        {
            PidId = DeserializePidId(answerText.Substring(3, 2));
            Data = DeserializeData(answerText, 6);
        }

        public int PidId { get; private set; }
        public byte[] Data { get; protected set; }


        public DateTime TimeStamp { get; private set; } = DateTime.Now;

        private int DeserializePidId(string pidIdStr)
        {
            int pidId = Convert.ToInt32(pidIdStr, 16); //throws FormatException when conversion fails
            return pidId;
        }
        protected byte[] DeserializeData(string dataStr, int startIndex)
        {
            dataStr = dataStr.Substring(startIndex, dataStr.Length - startIndex);
            return dataStr.Split(' ').Select(TryConvertToByte).Where(x => x.ConversionResult).Select(x => x.Output).ToArray();
        }

        private static (bool ConversionResult, byte Output) TryConvertToByte(string str)
        {
            if (byte.TryParse(str, NumberStyles.HexNumber, null, out var output))
                return (true, output);

            return (false, 0);
        }
    }

    public class DtcAnswer : ObdAnswer
    {
        public DtcAnswer(string answerText) : base(answerText)
        {
            Data = DeserializeData(answerText, 3);
        }

        public byte[] Data { get; protected set; }


        protected byte[] DeserializeData(string dataStr, int startIndex)
        {
            dataStr = dataStr.Substring(startIndex, dataStr.Length - startIndex);
            return dataStr.Replace(" ", "").Select(x => byte.Parse(x.ToString())).ToArray();
        }

    }

}