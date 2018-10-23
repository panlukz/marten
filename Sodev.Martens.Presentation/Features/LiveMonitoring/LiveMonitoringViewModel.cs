using Caliburn.Micro;
using Sodev.Marten.Base.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sodev.Marten.Base.Services;

namespace Sodev.Marten.Presentation.Features.LiveMonitoring
{
    public class LiveMonitoringViewModel : Screen
    {
        private readonly IEventAggregator eventAggregator;
        private readonly ILiveDataService liveDataService;
        private int numberOfRows = 1;
        private int numberOfColumns = 1;

        public LiveMonitoringViewModel(IEventAggregator eventAggregator, ILiveDataService liveDataService)
        {
            this.eventAggregator = eventAggregator;
            this.liveDataService = liveDataService;
  
            var speedMonitor = liveDataService.GetAvailableLiveMonitors().First(x => x.Id == 13);
            var rpmMonitor = liveDataService.GetAvailableLiveMonitors().First(x => x.Id == 12);
            var coolantTemp = liveDataService.GetAvailableLiveMonitors().First(x => x.Id == 5);
            var throttlePosition = liveDataService.GetAvailableLiveMonitors().First(x => x.Id == 17);

            LiveMonitorItems.Add(new LiveMonitorItemViewModel(speedMonitor, liveDataService)); //TODO temporary.

            LiveMonitorItems.Add(new LiveMonitorItemViewModel(rpmMonitor, liveDataService)); //TODO temporary.

            LiveMonitorItems.Add(new LiveMonitorItemViewModel(coolantTemp, liveDataService)); //TODO temporary.

            LiveMonitorItems.Add(new LiveMonitorItemViewModel(throttlePosition, liveDataService)); //TODO temporary.


        }

        public IList<LiveMonitorItemViewModel> LiveMonitorItems { get; private set; } = new List<LiveMonitorItemViewModel>(8);

        public int NumberOfRows
        {
            get => numberOfRows;
            set
            {
                numberOfRows = value;
                NotifyOfPropertyChange(nameof(NumberOfRows));
            }
        }

        public int NumberOfColumns
        {
            get => numberOfColumns;
            set
            {
                numberOfColumns = value;
                NotifyOfPropertyChange(nameof(NumberOfColumns));
            }
        }
        public IList<int> AvailableColumnsNumber
        {
            get
            {
                return Enumerable.Range(1, 2).ToList();
            }
        }

        public IList<int> AvailableRowsNumber
        {
            get
            {
                return Enumerable.Range(1, 4).ToList();
            }
        }

        private int maxVisibleGraphs => NumberOfColumns * NumberOfRows;

    }
}
