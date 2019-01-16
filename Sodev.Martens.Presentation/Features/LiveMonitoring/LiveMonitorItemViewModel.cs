using Caliburn.Micro;
using LiveCharts;
using System;
using System.Collections.Generic;
using System.Linq;
using Sodev.Marten.Domain.Model;
using Sodev.Marten.Domain.Services;
using Sodev.Marten.Presentation.Common;

namespace Sodev.Marten.Presentation.Features.LiveMonitoring
{
    public class LiveMonitorItemViewModel : PropertyChangedBase
    {
        private ILiveMonitor liveMonitor;
        private readonly ILiveDataService liveDataService;
        private readonly IEventAggregator eventAggregator = IoC.Get<IEventAggregator>(); //TODO :-(
        private Color strokeColor = Color.Green;

        public LiveMonitorItemViewModel(ILiveDataService liveDataService)
        {
            eventAggregator.Subscribe(this);
            this.liveMonitor = new EmptyLiveMonitor();
            this.liveDataService = liveDataService;
            ChartValues = new ChartValues<LiveDataModel>();
        }

        private void Data_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            ChartValues.AddRange(e.NewItems.Cast<object>());



            //TODO Probably it can't be just a fixed value (20). It has to be connected with the sample rate
            //Saying differently, how many reads we get in a know period of time
            if (ChartValues.Count > 40) ChartValues.RemoveAt(0);

            NotifyOfPropertyChange(() => MinXValue);
            NotifyOfPropertyChange(() => MaxXValue);
            NotifyOfPropertyChange(() => CurrentValue);
        }

        public ChartValues<LiveDataModel> ChartValues { get; private set; }

        public double CurrentValue => ChartValues.Count > 0 ? ChartValues.Last().Value : 0; 

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

        public List<ILiveMonitor> LiveMonitors => liveDataService.GetAvailableLiveMonitors().ToList();

        public ILiveMonitor SelectedLiveMonitor
        {
            get => liveMonitor;
            set
            {
                //TODO clean up this mess :-(
                if(value == liveMonitor) return;

                if (liveMonitor is LiveMonitor) //if it's not empty
                {
                    liveMonitor.Data.CollectionChanged -= Data_CollectionChanged;
                    ChartValues.Clear();
                    liveDataService.UnregisterLiveMonitor(liveMonitor);
                }
               
                liveMonitor = value;

                if (liveMonitor is LiveMonitor) //if it's not empty
                {
                    liveDataService.RegisterLiveMonitor(liveMonitor);
                    liveMonitor.Data.CollectionChanged += Data_CollectionChanged;
                }
                    
                NotifyOfPropertyChange(() => Unit);
                NotifyOfPropertyChange(() => MinValue);
                NotifyOfPropertyChange(() => MaxValue);
                NotifyOfPropertyChange(() => CanRemove);
            }
        }

        public Color StrokeColor
        {
            get => strokeColor;
            set
            {
                strokeColor = value;
                NotifyOfPropertyChange(() => StrokeColor);
            }
        }

        public MonitorType MonitorType => MonitorType.LiveData;

        public void Remove()
        {
            SelectedLiveMonitor = new EmptyLiveMonitor();
        }

        public bool CanRemove => !(liveMonitor is EmptyLiveMonitor);

    }
}
