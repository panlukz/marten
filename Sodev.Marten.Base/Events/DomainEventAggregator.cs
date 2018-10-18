using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sodev.Marten.Base.Events
{
    public class DomainEventAggregator : IDomainEventAggregator
    {
        public void PublishDomainEvent(DomainEventBase domainEvent)
        {
            if (DomainEventPublished == null)
                throw new InvalidOperationException("Can't publish domain event because nobody is listening! Domain event aggregator is not hooked up.");

            DomainEventPublished(this, domainEvent);
        }

        public delegate void DomainEventPublishedHandler(object sender, DomainEventBase domainEvent);

        public event DomainEventPublishedHandler DomainEventPublished;
    }

    public interface IDomainEventAggregator
    {
        void PublishDomainEvent(DomainEventBase domainEvent);

        event DomainEventAggregator.DomainEventPublishedHandler DomainEventPublished;
    }
}
