using Caliburn.Micro;
using LiveCharts;
using System;
using System.Linq;
using Sodev.Marten.Base.Model;

namespace Sodev.Marten.Presentation.Features.LiveMonitoring
{
    public class LiveMonitorItemViewModel : PropertyChangedBase
    {
        private readonly LiveMonitor liveMonitor;
        private readonly IEventAggregator eventAggregator = IoC.Get<IEventAggregator>(); //TODO :-(

        public LiveMonitorItemViewModel(LiveMonitor liveMonitor)
        {
            eventAggregator.Subscribe(this);
            this.liveMonitor = liveMonitor;
            ChartValues = new ChartValues<LiveDataModel>();
            liveMonitor.Data.CollectionChanged += Data_CollectionChanged;
        }

        private void Data_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            ChartValues.AddRange(e.NewItems.Cast<object>());

            if (ChartValues.Count > 20) ChartValues.RemoveAt(0);

            NotifyOfPropertyChange(() => MinXValue);
            NotifyOfPropertyChange(() => MaxXValue);
        }

        public ChartValues<LiveDataModel> ChartValues { get; private set; }

        public string Description => liveMonitor.Name;

        public string Unit => liveMonitor.Unit;

        public int MinValue => liveMonitor.MinValue;

        public int MaxValue => liveMonitor.MaxValue;

        public long MaxXValue
        {
            get
            {
                if (liveMonitor.Data.Count > 0)
                {
                    return liveMonitor.Data.Last().TimeSpan.Ticks + TimeSpan.FromSeconds(0.25d).Ticks;
                }
                else
                    return 0;
            }
        }

        public long MinXValue
        {
            get
            {
                if (liveMonitor.Data.Count > 0)
                {
                    return liveMonitor.Data.Last().TimeSpan.Ticks - (TimeSpan.FromSeconds(10)).Ticks;
                }
                else
                    return -1 * TimeSpan.FromSeconds(10).Ticks;
            }
        }

        public Func<double, string> DateTimeFormatter => value => value < 0 ? string.Empty : TimeSpan.FromTicks((long)value).ToString("c");

        public double AxisStep { get; } = TimeSpan.FromSeconds(1).Ticks;

        public double AxisUnit => TimeSpan.TicksPerSecond;

    }
}
