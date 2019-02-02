using Caliburn.Micro;
using Sodev.Marten.Base.Connection;
using System.Collections.Generic;
using System.Diagnostics;
using Sodev.Marten.Base.ObdCommunication;
using Sodev.Marten.Domain.Events;

namespace Sodev.Marten.Presentation.Features.Connection
{
    public class ConnectionViewModel : Screen
    {
        private readonly IObdCommuncation obdCommuncation;
        private readonly IEventAggregator eventAggregator;
        private string selectedPort;

        public ConnectionViewModel(IEventAggregator eventAggregator, IObdCommuncation obdCommuncation)
        {
            this.obdCommuncation = obdCommuncation;
            this.eventAggregator = eventAggregator;
            this.eventAggregator.Subscribe(this);
        }

        public IList<string> AvailablePorts => obdCommuncation.GetAvailablePorts();

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
            obdCommuncation.SetConnectionParameters(connectionParameters);

            obdCommuncation.ConnectionStateChanged += OnConnectionStateChanged; //TODO consider moving it to constructor?

            await obdCommuncation.OpenAsync();
            NotifyOfPropertyChange(() => IsConnected); //TODO hey maybe refresh it based on event sent by obdCommuncation service??
            NotifyOfPropertyChange(() => CanDisconnect);
        }

        private void OnConnectionStateChanged(object sender, ConnectionProcedureStateChangedPayload e)
        {
            CurrentConnectionProcedureProgress = e.Progress;
            CurrentConnectionProcedureStepDescription = e.Description;
            if(e.Progress == 100) //todo:fakeittillyoumakeit
                eventAggregator.PublishOnCurrentThread(new ConnectionStateChanged(ConnectionState.Opened));
            NotifyOfPropertyChange(() => CurrentConnectionProcedureProgress);
            NotifyOfPropertyChange(() => CurrentConnectionProcedureStepDescription);
            NotifyOfPropertyChange(nameof(CanConnectAsync));
            NotifyOfPropertyChange(nameof(CanDisconnect));
        }

        public string CurrentConnectionProcedureStepDescription { get; private set; }

        public int CurrentConnectionProcedureProgress { get; private set; }

        public void Disconnect()
        {
            obdCommuncation.Close();

            obdCommuncation.ConnectionStateChanged -= OnConnectionStateChanged;
            //todo: following event should be send from the lower layer
            eventAggregator.PublishOnCurrentThread(new ConnectionStateChanged(ConnectionState.Closed));
            CurrentConnectionProcedureProgress = 0;
            CurrentConnectionProcedureStepDescription = string.Empty;
            NotifyOfPropertyChange(() => CurrentConnectionProcedureProgress);
            NotifyOfPropertyChange(() => CurrentConnectionProcedureStepDescription);
            NotifyOfPropertyChange(() => IsConnected);
            NotifyOfPropertyChange(() => CanDisconnect);
            NotifyOfPropertyChange(nameof(CanConnectAsync));
        }

        public bool CanConnectAsync => obdCommuncation.ConnectionState == ConnectionState.Closed && !string.IsNullOrEmpty(SelectedPort);

        public bool CanDisconnect => obdCommuncation.ConnectionState == ConnectionState.Opened;

        public bool IsConnected => obdCommuncation.ConnectionState == ConnectionState.Opened;
    }
}
