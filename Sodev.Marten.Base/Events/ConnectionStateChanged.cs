using Sodev.Marten.Base.Connection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sodev.Marten.Base.Events
{
    public class ConnectionStateChanged : DomainEventBase
    {
        public ConnectionStateChanged(ConnectionState newState)
        {
            NewState = newState;
        }

        public ConnectionState NewState { get; }
    }
}
