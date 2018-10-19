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
using Sodev.Marten.Base.Model;

namespace Sodev.Marten.Presentation.Features.LiveMonitoring
{
    public class LiveMonitorItemViewModel : Screen
    {
        private readonly LiveMonitor internalLiveMonitor;

        public LiveMonitorItemViewModel(LiveMonitor liveMonitor)
        {
            internalLiveMonitor = liveMonitor;

            ChartValues = new ChartValues<double>();

            liveMonitor.Data.CollectionChanged += Data_CollectionChanged;
        }

        private void Data_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            ChartValues.AddRange(e.NewItems.Cast<object>());
        }

        public ChartValues<double> ChartValues { get; private set; }

    }
}
