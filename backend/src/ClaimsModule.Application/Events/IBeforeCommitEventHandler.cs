using ClaimsModule.Domain.Common.Events;

namespace ClaimsModule.Application.Events;

public interface IBeforeCommitEventHandler<in TEvent>
    where TEvent : DomainEvent
{
    Task HandleAsync(TEvent domainEvent, EventContext context, CancellationToken cancellationToken);
}
