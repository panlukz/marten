using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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

        public ObservableCollection<double> Data { get; } = new ObservableCollection<double>();

        internal void UpdateData(byte[] newData)
        {
            var translatedData = internalPid.GetValue(newData);
            Data.Add(translatedData);
            Debug.WriteLine(translatedData);
        }
    }
}
