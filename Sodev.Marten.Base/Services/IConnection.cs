using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sodev.Marten.Base.Connection;

namespace Sodev.Marten.Base.Services
{
    public interface IConnection
    {
        Task OpenAsync();

        void SetParameters(ConnectionParameters parameters);

        void Close();

        void CloseOnError();

        ConnectionState GetState();

        IList<string> GetAvailablePorts();

        void WriteData(string data);

        event EventHandler<string> DataReceived;

        event EventHandler<ConnectionProcedureStateChangedPayload> StateChanged;
    }

    public interface IConnectionInfo
    {
        ConnectionState GetState();
    }
}
