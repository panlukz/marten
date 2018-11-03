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

        ConnectionState GetState();

        IList<string> GetAvailablePorts();

        void WriteData(string data);

        event EventHandler<string> DataReceived;

    }

    public interface IConnectionInfo
    {
        ConnectionState GetState();
    }
}
