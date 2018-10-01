using org.mariuszgromada.math.mxparser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sodev.Marten.Base.Model
{
    public class Pid<T> where T : new()
    {
        private Pid() {}

        public int Id { private set; get; }

        public string Description { private set; get; }

        public int ReturnedBytesNb { private set; get; }

        public int MinValue { private set; get; }

        public int MaxValue { private set; get; }

        public string Unit { private set; get; }

        public string Formula { private set; get; }

        public T GetValue(byte[] arguments)
        {
            return new T();
        }

        public static Pid<T> Create(PidParameters parameters)
        {
            return new Pid<T>()
            {
                Id = parameters.Id,
                Description = parameters.Description,
                ReturnedBytesNb = parameters.ReturnedBytesNb,
                MinValue = parameters.MinValue,
                MaxValue = parameters.MaxValue,
                Unit = parameters.Unit,
                Formula = parameters.Formula
            };

            Func<double> ConvertStringFormulaToAction(string formula)
            {
                var A = new Argument("A", 4);
                Expression e = new Expression(formula, A);
                return () => e.calculate();
            }
        }
    }
}
