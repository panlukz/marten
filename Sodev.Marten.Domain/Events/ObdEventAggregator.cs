using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sodev.Marten.Domain.Events
{
    public class ObdEventBus : IObdEventBus
    {
        public void PublishEvent(ObdEventBase domainEvent)
        {
            if (OnEventPublished == null)
                throw new InvalidOperationException("Can't publish domain event because nobody is listening! Domain event aggregator is not hooked up.");

            OnEventPublished(this, domainEvent);
        }

        public delegate void DomainEventPublishedHandler(object sender, ObdEventBase domainEvent);

        public event DomainEventPublishedHandler OnEventPublished;
    }

    public interface IObdEventBus
    {
        void PublishEvent(ObdEventBase domainEvent);

        event ObdEventBus.DomainEventPublishedHandler OnEventPublished;
    }

}
