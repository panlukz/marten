using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Navigation;
using Caliburn.Micro;
using Sodev.Marten.Presentation.Interfaces;
using Sodev.Marten.Presentation.Events;

namespace Sodev.Marten.Presentation.Services
{
    public class NavigationFlowService : INavigationFlowService
    {
        private readonly IEventAggregator eventAggregator;

        public NavigationFlowService(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
        }

        public void OpenFeature(Type featureType)
        {
            if (!featureType.IsSubclassOf(typeof(Screen)))
                throw new Exception("Feature type has to derrive from Screen class");


            eventAggregator.PublishOnUIThread(new NavigationEvent(featureType));
        }
    }
}
