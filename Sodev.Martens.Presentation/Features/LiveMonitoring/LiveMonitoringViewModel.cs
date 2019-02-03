using Caliburn.Micro;
using Sodev.Marten.Base.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sodev.Marten.Domain.Services;

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

            for (int i = 0; i < 8; i++) //TODO get rid of fixed numbers (8)
            {
                LiveMonitorItems.Add(new LiveMonitorItemViewModel(liveDataService));
            }
        }

        protected override void OnActivate()
        {
            liveDataService.SubscribeLiveDataUpdatedEvent();
            base.OnActivate();
        }

        protected override void OnDeactivate(bool close)
        {
            foreach (var monitor in LiveMonitorItems)
            {
                monitor.Remove();
            }
            liveDataService.UnsubscribeLiveDataUpdatedEvent();
            base.OnDeactivate(close);
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
