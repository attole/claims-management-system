using ClaimsModule.Domain.Common.Events;

namespace ClaimsModule.Application.Events;

public interface IInTransactionEventHandler<in TEvent>
    where TEvent : DomainEvent
{
    Task HandleAsync(TEvent domainEvent, EventContext context, CancellationToken cancellationToken);
}
