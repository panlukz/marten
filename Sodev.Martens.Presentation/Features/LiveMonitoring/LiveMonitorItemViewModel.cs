using Caliburn.Micro;
using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Sodev.Marten.Base.Events;
using Sodev.Marten.Base.Model;

namespace Sodev.Marten.Presentation.Features.LiveMonitoring
{
    public class LiveMonitorItemViewModel : PropertyChangedBase, IHandle<StartQueryingEvent>
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

            NotifyOfPropertyChange(() => MinXValue);
            NotifyOfPropertyChange(() => MaxXValue);
        }

        public ChartValues<LiveDataModel> ChartValues { get; private set; }

        public string Description => liveMonitor.Name;

        public string Unit => liveMonitor.Unit;

        public int MinValue => liveMonitor.MinValue;

        public int MaxValue => liveMonitor.MaxValue;

        public DateTime MeasuringStartTime { get; private set; }

        public double MaxXValue
        {
            get
            {
                if (liveMonitor.Data.Count > 0)
                {
                    return liveMonitor.Data.Last().TimeSpan.Ticks - MeasuringStartTime.Ticks + TimeSpan.FromSeconds(1).Ticks;
                }
                else
                    return (TimeSpan.FromSeconds(20)).Ticks;
            }
        }

        public double MinXValue
        {
            get
            {
                if (liveMonitor.Data.Count > 0)
                {
                    return liveMonitor.Data.Last().TimeSpan.Ticks;
                }
                else
                    return MeasuringStartTime.Ticks;
            }
        }

        public Func<double, string> DateTimeFormatter => value => (new DateTime((long)value) - MeasuringStartTime).ToString();

        public double AxisStep { get; } = TimeSpan.FromSeconds(1).Ticks;

        public double AxisUnit => TimeSpan.TicksPerSecond;


        public void Handle(StartQueryingEvent message)
        {
            MeasuringStartTime = message.TimeStamp;
            NotifyOfPropertyChange(() => MeasuringStartTime);
            NotifyOfPropertyChange(() => MaxXValue);
            NotifyOfPropertyChange(() => MinXValue);
            NotifyOfPropertyChange(() => DateTimeFormatter);



        }
    }
}
