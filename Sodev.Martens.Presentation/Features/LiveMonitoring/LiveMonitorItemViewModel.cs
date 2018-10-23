using Caliburn.Micro;
using LiveCharts;
using System;
using System.Collections.Generic;
using System.Linq;
using Sodev.Marten.Base.Model;
using Sodev.Marten.Base.Services;

namespace Sodev.Marten.Presentation.Features.LiveMonitoring
{
    public class LiveMonitorItemViewModel : PropertyChangedBase
    {
        private LiveMonitor liveMonitor;
        private readonly ILiveDataService liveDataService;
        private readonly IEventAggregator eventAggregator = IoC.Get<IEventAggregator>(); //TODO :-(

        public LiveMonitorItemViewModel(LiveMonitor liveMonitor, ILiveDataService liveDataService)
        {
            eventAggregator.Subscribe(this);
            this.liveMonitor = liveMonitor;
            this.liveDataService = liveDataService;


            liveDataService.RegisterLiveMonitor(liveMonitor);
            ChartValues = new ChartValues<LiveDataModel>();
            liveMonitor.Data.CollectionChanged += Data_CollectionChanged;
        }

        private void Data_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            ChartValues.AddRange(e.NewItems.Cast<object>());

            //TODO Probably it can't be just a fixed value (20). It has to be connected with the sample rate
            //Saying differently, how many reads we get in a know period of time
            if (ChartValues.Count > 20) ChartValues.RemoveAt(0);

            NotifyOfPropertyChange(() => MinXValue);
            NotifyOfPropertyChange(() => MaxXValue);
        }

        public ChartValues<LiveDataModel> ChartValues { get; private set; }

        //public string Description => liveMonitor.Name; TODO probably not needed

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

        public List<LiveMonitor> LiveMonitors => liveDataService.GetAvailableLiveMonitors().ToList();

        public LiveMonitor SelectedLiveMonitor
        {
            get => liveMonitor;
            set
            {
                if(value == liveMonitor) return;

                liveMonitor.Data.CollectionChanged -= Data_CollectionChanged;
                ChartValues.Clear();
                liveDataService.UnregisterLiveMonitor(liveMonitor);
                liveMonitor = value;
                liveDataService.RegisterLiveMonitor(liveMonitor);
                liveMonitor.Data.CollectionChanged += Data_CollectionChanged;

                NotifyOfPropertyChange(() => Unit);
                NotifyOfPropertyChange(() => MinValue);
                NotifyOfPropertyChange(() => MaxValue);
            }
        }

    }
}
