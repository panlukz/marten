using Sodev.Marten.Base.Connection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sodev.Marten.Domain.Events
{
    public class ConnectionStateChanged : ObdEventBase
    {
        public ConnectionStateChanged(ConnectionState newState)
        {
            NewState = newState;
        }

        public ConnectionState NewState { get; }
    }
}
