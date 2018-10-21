using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sodev.Marten.Base.Events;

namespace Sodev.Marten.Base.Events
{
    public class StartQueryingEvent : DomainEventBase
    {
        public DateTime TimeStamp { get; }

        public StartQueryingEvent(DateTime timeStamp)
        {
            this.TimeStamp = timeStamp;
        }
    }
}
