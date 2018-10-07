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
        public LiveMonitoringViewModel()
        {
            LiveMonitorItems.Add(new LiveMonitorItemViewModel()); //TODO temporary.
        }

        public Pid Pid { get; private set; }

        public IList<LiveMonitorItemViewModel> LiveMonitorItems { get; private set; } = new List<LiveMonitorItemViewModel>();
    }
}
