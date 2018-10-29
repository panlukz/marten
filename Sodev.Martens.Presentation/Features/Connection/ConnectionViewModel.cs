using Caliburn.Micro;
using Sodev.Marten.Base.Connection;
using Sodev.Marten.Base.Events;
using Sodev.Marten.Base.Services;
using System.Collections.Generic;

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
                NotifyOfPropertyChange(() => CanConnectAsync);
            }
        }


        //TODO get rid of these magic numbers
        public IList<int> AvailableBaudRates { get; } = new List<int> {1200, 2400, 4800, 9600, 19200, 38400, 57600, 115200};

        public int SelectedBaudRate { get; set; } = 9600;

        public async void ConnectAsync()
        {
            var connectionParameters = new ConnectionParameters
            {
                PortName = SelectedPort,
                BaudRate = SelectedBaudRate
            };
            connectionService.SetParameters(connectionParameters);
            await connectionService.OpenAsync();
            NotifyOfPropertyChange(() => IsConnected); //TODO hey maybe refresh it based on event sent by connection service??
            NotifyOfPropertyChange(() => CanDisconnect);
        }

        public void Disconnect()
        {
            connectionService.Close();
            NotifyOfPropertyChange(() => IsConnected);
            NotifyOfPropertyChange(() => CanDisconnect);
        }

        public bool CanConnectAsync => connectionService.GetState() == ConnectionState.Closed && !string.IsNullOrEmpty(SelectedPort);

        public bool CanDisconnect => connectionService.GetState() == ConnectionState.Opened;

        public bool IsConnected => connectionService.GetState() == ConnectionState.Opened;

        public void Handle(ConnectionStateChanged message)
        {
            NotifyOfPropertyChange(() => CanConnectAsync);
        }
    }
}
