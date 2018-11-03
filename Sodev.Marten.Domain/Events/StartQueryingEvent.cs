using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sodev.Marten.Domain.Events;

namespace Sodev.Marten.Domain.Events
{
    public class StartQueryingEvent : ObdEventBase
    {
        public DateTime TimeStamp { get; }

        public StartQueryingEvent(DateTime timeStamp)
        {
            this.TimeStamp = timeStamp;
        }
    }
}
