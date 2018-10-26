using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sodev.Marten.Base.Model
{
    public class LiveMonitor : ILiveMonitor
    {
        public LiveMonitor(Pid pid) => internalPid = pid;

        private readonly Pid internalPid;

        public int Id => internalPid.Id;

        public string Name => internalPid.Description;

        public int MinValue => internalPid.MinValue;

        public int MaxValue => internalPid.MaxValue;

        public string Unit => internalPid.Unit;

        public ObservableCollection<LiveDataModel> Data { get; } = new ObservableCollection<LiveDataModel>();

        public void UpdateData(byte[] newData, TimeSpan timeSpan)
        {
            var translatedData = internalPid.GetValue(newData);
            var model = new LiveDataModel(translatedData, timeSpan);
            Data.Add(model);
            Debug.WriteLine(translatedData);
        }
    }

    public class EmptyLiveMonitor : ILiveMonitor
    {
        public int Id => -1;

        public string Name => string.Empty;

        public int MinValue => 0;

        public int MaxValue => 100;

        public string Unit => string.Empty;

        public ObservableCollection<LiveDataModel> Data { get; } = new ObservableCollection<LiveDataModel>();

        public void UpdateData(byte[] newData, TimeSpan timeSpan)
        {
            //Do nothing TODO probably breaks one of the SOLID principles regarding interfaces. I'm sorry Uncle Bob ;-(
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
