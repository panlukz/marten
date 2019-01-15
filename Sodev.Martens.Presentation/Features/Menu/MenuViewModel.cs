using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sodev.Marten.Presentation.Events;
using Sodev.Marten.Presentation.Features.Connection;
using Sodev.Marten.Presentation.Features.FaultCodes;
using Sodev.Marten.Presentation.Features.LiveMonitoring;
using Sodev.Marten.Presentation.Features.Preferences;

namespace Sodev.Marten.Presentation.Features.Menu
{
    public class MenuViewModel : Screen
    {
        private readonly IEventAggregator eventAggregator;

        public MenuViewModel()
        {
            this.eventAggregator = IoC.Get<IEventAggregator>(); //TODO fix that
        }

        public void OpenConnection()
        {
            eventAggregator.PublishOnUIThread(new NavigationEvent(typeof(ConnectionViewModel)));
        }

        public void OpenLiveData()
        {
            eventAggregator.PublishOnUIThread(new NavigationEvent(typeof(LiveMonitoringViewModel)));
        }

        public void OpenCodes()
        {
            eventAggregator.PublishOnUIThread(new NavigationEvent(typeof(FaultCodesViewModel)));
        }

        public void OpenPreferences()
        {
            eventAggregator.PublishOnUIThread(new NavigationEvent(typeof(PreferencesViewModel)));
        }
    }
}
