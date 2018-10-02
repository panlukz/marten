using org.mariuszgromada.math.mxparser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sodev.Marten.Base.Model
{
    public class Pid
    {
        private Pid() {}

        public int Id { private set; get; }

        public string Description { private set; get; }

        public int ReturnedBytesNb { private set; get; }

        public int MinValue { private set; get; }

        public int MaxValue { private set; get; }

        public string Unit { private set; get; }

        private string formula;

        public double GetValue(byte[] bytesArgs)
        {
            var arguments = DeserializeArgumentsFromBytes(bytesArgs);
            return new Expression(formula, arguments).calculate();

            Argument[] DeserializeArgumentsFromBytes(byte[] bytesArray)
            {
                var argumentsArray = new Argument[4];
                //TODO To implement...
                return argumentsArray;
            }
        }

        public static Pid Create(PidParameters parameters)
        {
            return new Pid()
            {
                Id = parameters.Id,
                Description = parameters.Description,
                ReturnedBytesNb = parameters.ReturnedBytesNb,
                MinValue = parameters.MinValue,
                MaxValue = parameters.MaxValue,
                Unit = parameters.Unit,
                formula = parameters.Formula
            };
        }
    }
}
