using System;
using System.Collections.Generic;
using Caliburn.Micro;
using Sodev.Marten.Base.Connection;
using Sodev.Marten.Base.Events;
using Sodev.Marten.Base.Services;
using Sodev.Marten.Presentation.Features.Connection;
using Sodev.Marten.Presentation.Features.LiveMonitoring;
using Sodev.Marten.Presentation.Features.Preferences;
using Sodev.Marten.Presentation.Features.Shell;
using Sodev.Marten.Presentation.Interfaces;
using Sodev.Marten.Presentation.Services;

namespace Sodev.Marten.Presentation.Bootstrapper
{
    public class Bootstrapper : BootstrapperBase, IServiceLocator
    {
        SimpleContainer container;

        public Bootstrapper() {
            Initialize();
        }

        protected override void Configure()
        {
            container = new SimpleContainer();
            container.PerRequest<ConnectionViewModel>();
            container.Singleton<LiveMonitoringViewModel>();
            container.PerRequest<PreferencesViewModel>();


            container.Singleton<IWindowManager, WindowManager>();
            container.RegisterInstance(typeof(IServiceLocator), string.Empty, this);
            container.Singleton<IEventAggregator, EventAggregator>();
            container.Singleton<IDomainEventAggregator, DomainEventAggregator>();
            container.Singleton<INavigationFlowService, NavigationFlowService>();
            container.Singleton<IConnectionService, Connection>();
            container.Singleton<ILiveDataService, LiveDataService>();

            container.PerRequest<ShellViewModel>();

            ConfigureDomainEventAggregator();
        }

        private void ConfigureDomainEventAggregator()
        {
            var domainEventAggregator = container.GetInstance<IDomainEventAggregator>();
            domainEventAggregator.DomainEventPublished += DomainEventAggregator_DomainEventPublished;
        }

        private void DomainEventAggregator_DomainEventPublished(object sender, DomainEventBase domainEvent)
        {
            container.GetInstance<IEventAggregator>().PublishOnUIThread(domainEvent);
        }

        protected override object GetInstance(Type service, string key) => container.GetInstance(service, key);

        protected override IEnumerable<object> GetAllInstances(Type service) => container.GetAllInstances(service);

        protected override void BuildUp(object instance) => container.BuildUp(instance);

        protected override void OnStartup(object sender, System.Windows.StartupEventArgs e) => DisplayRootViewFor<ShellViewModel>();

        public object GetInstance(Type type) => container.GetInstance(type, string.Empty);

    }
}