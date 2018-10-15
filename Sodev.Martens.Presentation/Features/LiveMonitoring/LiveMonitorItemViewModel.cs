using Caliburn.Micro;
using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sodev.Marten.Presentation.Features.LiveMonitoring
{
    public class LiveMonitorItemViewModel : Screen
    {
        public LiveMonitorItemViewModel()
        {
            Series = new SeriesCollection
            {
                new LineSeries
                {
                    Values = new ChartValues<double> { 3, 5, 7, 4 }
                }
            };
        }

        public SeriesCollection Series { get; private set; }

    }
}
