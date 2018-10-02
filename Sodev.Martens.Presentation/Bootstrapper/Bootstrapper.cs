using System;
using System.Collections.Generic;
using Caliburn.Micro;

namespace Sodev.Marten.Presentation.Bootstrapper
{
    public class Bootstrapper : BootstrapperBase
    {
        SimpleContainer container;

        public Bootstrapper() {
            Initialize();
        }

        protected override void Configure()
        {
            container = new SimpleContainer();

            container.Singleton<IWindowManager, WindowManager>();
            container.Singleton<IEventAggregator, EventAggregator>();
            container.PerRequest<IShell, ShellViewModel>();
        }

        protected override object GetInstance(Type service, string key) => container.GetInstance(service, key);

        protected override IEnumerable<object> GetAllInstances(Type service) => container.GetAllInstances(service);

        protected override void BuildUp(object instance) => container.BuildUp(instance);

        protected override void OnStartup(object sender, System.Windows.StartupEventArgs e) => DisplayRootViewFor<IShell>();
    }
}