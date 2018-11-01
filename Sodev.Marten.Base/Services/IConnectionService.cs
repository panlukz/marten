﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sodev.Marten.Base.Connection;

namespace Sodev.Marten.Base.Services
{
    public interface IConnectionService
    {
        Task OpenAsync();

        void SetParameters(ConnectionParameters parameters);

        void Close();

        ConnectionState GetState();

        IList<string> GetAvailablePorts();

        void SendQuery(ObdQuery query);

        event EventHandler<ObdAnswer> AnswerReceivedEvent;
    }

    public interface IConnectionInfo
    {
        ConnectionState GetState();
    }
}
