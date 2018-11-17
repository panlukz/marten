﻿using Caliburn.Micro;
using Sodev.Marten.Base.Connection;
using System.Collections.Generic;
using System.Diagnostics;
using Sodev.Marten.Base.ObdCommunication;
using Sodev.Marten.Domain.Events;

namespace Sodev.Marten.Presentation.Features.Connection
{
    public class ConnectionViewModel : Screen, IHandle<ConnectionStateChanged>
    {
        private readonly IObdCommuncation obdCommuncation;
        private string selectedPort;

        public ConnectionViewModel(IEventAggregator eventAggregator, IObdCommuncation obdCommuncation)
        {
            this.obdCommuncation = obdCommuncation;
            eventAggregator.Subscribe(this);
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
            NotifyOfPropertyChange(() => CurrentConnectionProcedureProgress);
            NotifyOfPropertyChange(() => CurrentConnectionProcedureStepDescription);
        }

        public string CurrentConnectionProcedureStepDescription { get; private set; }

        public int CurrentConnectionProcedureProgress { get; private set; }

        public void Disconnect()
        {
            obdCommuncation.Close();

            obdCommuncation.ConnectionStateChanged -= OnConnectionStateChanged;

            NotifyOfPropertyChange(() => IsConnected);
            NotifyOfPropertyChange(() => CanDisconnect);
        }

        public bool CanConnectAsync => obdCommuncation.ConnectionState == ConnectionState.Closed && !string.IsNullOrEmpty(SelectedPort);

        public bool CanDisconnect => obdCommuncation.ConnectionState == ConnectionState.Opened;

        public bool IsConnected => obdCommuncation.ConnectionState == ConnectionState.Opened;

        public void Handle(ConnectionStateChanged message)
        {
            NotifyOfPropertyChange(() => CanConnectAsync);
        }
    }
}
