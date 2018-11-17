using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using Sodev.Marten.Presentation.Events;
using Sodev.Marten.Presentation.Features.Connection;
using Sodev.Marten.Presentation.Features.Dummy;
using Sodev.Marten.Presentation.Features.LiveMonitoring;
using Sodev.Marten.Presentation.Features.Preferences;
using Sodev.Marten.Presentation.Interfaces;

namespace Sodev.Marten.Presentation.Features.Shell
{
    internal class ShellViewModel : Conductor<object>, IHandle<NavigationEvent>
    {
        private readonly IEventAggregator eventAggregator;
        private readonly IServiceLocator serviceLocator;

        public ShellViewModel(IEventAggregator eventAggregator, IServiceLocator serviceLocator)
        {
            this.eventAggregator = eventAggregator;
            this.serviceLocator = serviceLocator;
            eventAggregator.Subscribe(this);

            ActivateItem(IoC.Get<ConnectionViewModel>()); //TODO temporary. remove it.

        }

        public void Handle(NavigationEvent navigationEvent)
        {
            var featureType = navigationEvent.FeatureType;
            ActivateItem(IoC.GetInstance(featureType, null));
        }
    }
}
