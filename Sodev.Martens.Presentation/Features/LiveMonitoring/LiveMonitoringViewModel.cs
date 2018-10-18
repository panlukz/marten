using Caliburn.Micro;
using Sodev.Marten.Base.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sodev.Marten.Presentation.Features.LiveMonitoring
{
    class LiveMonitoringViewModel : Screen
    {
        private readonly IEventAggregator eventAggregator;
        private int numberOfRows = 1;
        private int numberOfColumns = 1;

        public LiveMonitoringViewModel(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
            LiveMonitorItems.Add(new LiveMonitorItemViewModel()); //TODO temporary.
            LiveMonitorItems.Add(new LiveMonitorItemViewModel()); //TODO temporary.
            LiveMonitorItems.Add(new LiveMonitorItemViewModel()); //TODO temporary.
        }

        public IList<LiveMonitorItemViewModel> LiveMonitorItems { get; private set; } = new List<LiveMonitorItemViewModel>();

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
