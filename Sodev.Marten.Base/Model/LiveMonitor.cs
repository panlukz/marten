using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sodev.Marten.Base.Model
{
    public class LiveMonitor
    {
        public LiveMonitor(Pid pid) => internalPid = pid;

        private readonly Pid internalPid;

        public int Id => internalPid.Id;

        public string Name => internalPid.Description;

        public int MinValue => internalPid.MinValue;

        public int MaxValue => internalPid.MaxValue;

        public string Unit => internalPid.Unit;

        public IList<double> Data { get; } = new List<double>();

        internal void UpdateData(double newData) => Data.Add(newData);
    }
}
