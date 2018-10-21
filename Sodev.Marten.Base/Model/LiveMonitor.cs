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

        public ObservableCollection<LiveDataModel> Data { get; } = new ObservableCollection<LiveDataModel>();

        internal void UpdateData(byte[] newData, TimeSpan timeSpan)
        {
            var translatedData = internalPid.GetValue(newData);
            var model = new LiveDataModel(translatedData, timeSpan);
            Data.Add(model);
            Debug.WriteLine(translatedData);
        }
    }

    public class LiveDataModel
    {
        public LiveDataModel(double value, TimeSpan timeSpan)
        {
            Value = value;
            TimeSpan = timeSpan;
        }

        public double Value { get; private set; }
        public TimeSpan TimeSpan { get; private set; }
    }
}
