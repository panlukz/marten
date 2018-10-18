using Caliburn.Micro;
using Sodev.Marten.Base.Connection;
using Sodev.Marten.Base.Events;
using Sodev.Marten.Base.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sodev.Marten.Presentation.Features.Connection
{
    public class ConnectionViewModel : Screen, IHandle<ConnectionStateChanged>
    {
        private readonly IConnectionService connectionService;
        private string selectedPort;

        public ConnectionViewModel(IEventAggregator eventAggregator, IConnectionService connectionService)
        {
            this.connectionService = connectionService;
            eventAggregator.Subscribe(this);
        }

        public IList<string> AvailablePorts => connectionService.GetAvailablePorts();

        public string SelectedPort
        {
            get => selectedPort;
            set
            {
                selectedPort = value;
                NotifyOfPropertyChange(() => CanConnect);
            }
        }


        //TODO get rid of these magic numbers
        public IList<int> AvailableBaudRates { get; } = new List<int> {1200, 2400, 4800, 9600, 19200, 38400, 57600, 115200};

        public int SelectedBaudRate { get; set; } = 9600;

        public void Connect()
        {
            var connectionParameters = new ConnectionParameters
            {
                PortName = SelectedPort,
                BaudRate = SelectedBaudRate
            };
            connectionService.SetParameters(connectionParameters);
            connectionService.Open();
        }

        public bool CanConnect => connectionService.GetState() == ConnectionState.Closed && !string.IsNullOrEmpty(SelectedPort);

        public void Handle(ConnectionStateChanged message)
        {
            NotifyOfPropertyChange(() => CanConnect);
        }
    }
}
