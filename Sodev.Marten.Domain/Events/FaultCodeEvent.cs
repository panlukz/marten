using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sodev.Marten.Domain.Events
{
    public class FaultCodeEvent : ObdEventBase
    {
        public FaultCodeEventType EventType { get; }

        public FaultCodeEvent(FaultCodeEventType eventType)
        {
            EventType = eventType;
        }
    }

    public enum FaultCodeEventType
    {
        FaultCodesCleared,
        FaultCodesRetrieved,
        FaultCodesNumberUpdated
    }

}
