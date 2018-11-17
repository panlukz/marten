using System;
using System.Globalization;
using System.Linq;
using Sodev.Marten.Base.ExceptionHandling;

namespace Sodev.Marten.Base.Connection
{
    public class ObdAnswer
    {
        public ObdAnswer(string answerText)
        {
            ServiceNb = DeserializeServiceNb(answerText.Substring(1, 1));
            PidId = DeserializePidId(answerText.Substring(3, 2));
            Data = DeserializeData(answerText.Substring(6, answerText.Length - 6));
        }

        public int ServiceNb { get; private set; }

        public int PidId { get; private set; }

        public byte[] Data { get; private set; }

        public DateTime TimeStamp { get; private set; } = DateTime.Now;

        private int DeserializeServiceNb(string serviceStr)
        {
            int serviceNb = Convert.ToInt32(serviceStr); //throws FormatException when conversion fails
            if(serviceNb < 1 || serviceNb > 9) throw new FormatException($"Service should be a number within 1-9 range. It was: {serviceNb}");
            
            return serviceNb;
        }

        private int DeserializePidId(string pidIdStr)
        {
            int pidId = Convert.ToInt32(pidIdStr, 16); //throws FormatException when conversion fails
            return pidId;
        }

        private byte[] DeserializeData(string dataStr) => 
            dataStr.Split(' ').Select(TryConvertToByte).Where(x => x.ConversionResult).Select(x => x.Output).ToArray();

        private static (bool ConversionResult, byte Output) TryConvertToByte(string str)
        {
            if (byte.TryParse(str, NumberStyles.HexNumber, null, out var output))
                return (true, output);

            return (false, 0);
        }

    }
}