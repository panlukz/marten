using System;
using System.Collections.ObjectModel;

namespace Sodev.Marten.Domain.Model
{
    public interface ILiveMonitor
    {
        ObservableCollection<LiveDataModel> Data { get; }
        int Id { get; }
        int MaxValue { get; }
        int MinValue { get; }
        string Name { get; }
        string Unit { get; }
        void UpdateData(byte[] newData, TimeSpan timeSpan);
    }
}