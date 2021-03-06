using System;
using System.Collections.Generic;
using Caliburn.Micro;
using LiveCharts.Configurations;
using Sodev.Marten.Domain.Events;
using Sodev.Marten.Base.ObdCommunication;
using Sodev.Marten.Base.Services;
using Sodev.Marten.Domain.Model;
using Sodev.Marten.Domain.Services;
using Sodev.Marten.Presentation.Features.Connection;
using Sodev.Marten.Presentation.Features.FaultCodes;
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
            container.Singleton<ConnectionViewModel>();
            container.Singleton<LiveMonitoringViewModel>();
            container.PerRequest<PreferencesViewModel>();
            container.PerRequest<FaultCodesViewModel>();
            container.PerRequest<ShellViewModel>();


            container.Singleton<IWindowManager, WindowManager>();
            container.RegisterInstance(typeof(IServiceLocator), string.Empty, this);
            container.Singleton<IEventAggregator, EventAggregator>();
            container.Singleton<IDialogService, DialogService>();
            container.Singleton<IObdEventBus, ObdEventBus>();
            container.Singleton<IObdCommuncation, ObdSerialCommunication>();
            container.Singleton<INavigationFlowService, NavigationFlowService>();
            container.Singleton<ILiveDataService, LiveDataService>();
            container.Singleton<IFaultCodesService, FaultCodesService>();
            container.Singleton<IPidRepository, PidRepository>();


            ConfigureDomainEventAggregator();

            LiveCharts.Charting.For<LiveDataModel>(Mappers.Xy<LiveDataModel>()
                .X(x => x.TimeSpan.Ticks)
                .Y(y => y.Value));
        }

        private void ConfigureDomainEventAggregator()
        {
            var obdEventBus = container.GetInstance<IObdEventBus>();
            obdEventBus.OnEventPublished += OnEventAggregatorOnEventPublished;
        }

        private void OnEventAggregatorOnEventPublished(object sender, ObdEventBase domainEvent)
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