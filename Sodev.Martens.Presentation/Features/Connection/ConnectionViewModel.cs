using Caliburn.Micro;
using Sodev.Marten.Base.Connection;
using System.Collections.Generic;
using Sodev.Marten.Base.ObdCommunication;
using Sodev.Marten.Domain.Events;
using System;
using Sodev.Marten.Presentation.Interfaces;

namespace Sodev.Marten.Presentation.Features.Connection
{
    public class ConnectionViewModel : Screen
    {
        private readonly IObdCommuncation obdCommuncation;
        private readonly IDialogService dialogService;
        private readonly IEventAggregator eventAggregator;
        private string selectedPort;

        public ConnectionViewModel(IEventAggregator eventAggregator, IObdCommuncation obdCommuncation, IDialogService dialogService)
        {
            this.obdCommuncation = obdCommuncation;
            this.dialogService = dialogService;
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
        public IList<int> AvailableBaudRates { get; } = new List<int> { 1200, 2400, 4800, 9600, 19200, 38400, 57600, 115200 };

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

            //TODO move the following try-catch to the lower layer
            try
            {
                await obdCommuncation.OpenAsync();
            }
            catch (Exception ex)
            {
                dialogService.DisplayError(ex.Message, "Connection error");
                DisconnectOnError();
            }

            NotifyOfPropertyChange(() => IsConnected); //TODO hey maybe refresh it based on event sent by obdCommuncation service??
            NotifyOfPropertyChange(nameof(CanConnectAsync));
            NotifyOfPropertyChange(() => CanDisconnectOnDemand);
        }

        private void OnConnectionStateChanged(object sender, ConnectionProcedureStateChangedPayload e)
        {
            CurrentConnectionProcedureProgress = e.Progress;
            CurrentConnectionProcedureStepDescription = e.Description;
            if (e.Progress == 100) //todo:fakeittillyoumakeit
                eventAggregator.PublishOnCurrentThread(new ConnectionStateChanged(ConnectionState.Opened));
            NotifyOfPropertyChange(() => CurrentConnectionProcedureProgress);
            NotifyOfPropertyChange(() => CurrentConnectionProcedureStepDescription);
            NotifyOfPropertyChange(nameof(CanConnectAsync));
            NotifyOfPropertyChange(nameof(CanDisconnectOnDemand));
        }

        public string CurrentConnectionProcedureStepDescription { get; private set; }

        public int CurrentConnectionProcedureProgress { get; private set; }

        public void DisconnectOnError()
        {
            obdCommuncation.CloseOnError();
            Disconnect();
        }

        public void DisconnectOnDemand()
        {
            obdCommuncation.Close();
            Disconnect();
        }

        private void Disconnect()
        {
            obdCommuncation.ConnectionStateChanged -= OnConnectionStateChanged;
            //todo: following event should be send from the lower layer
            eventAggregator.PublishOnCurrentThread(new ConnectionStateChanged(ConnectionState.Closed));
            CurrentConnectionProcedureProgress = 0;
            CurrentConnectionProcedureStepDescription = string.Empty;
            NotifyOfPropertyChange(() => CurrentConnectionProcedureProgress);
            NotifyOfPropertyChange(() => CurrentConnectionProcedureStepDescription);
            NotifyOfPropertyChange(() => IsConnected);
            NotifyOfPropertyChange(() => CanDisconnectOnDemand);
            NotifyOfPropertyChange(nameof(CanConnectAsync));
        }

        public bool CanConnectAsync => obdCommuncation.ConnectionState == ConnectionState.Closed && !string.IsNullOrEmpty(SelectedPort);

        public bool CanDisconnectOnDemand => obdCommuncation.ConnectionState == ConnectionState.Opened;

        public bool IsConnected => obdCommuncation.ConnectionState == ConnectionState.Opened;
    }
}
