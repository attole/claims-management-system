using ClaimsModule.Domain.Common.Events;

namespace ClaimsModule.Application.Events.Contracts;

public interface IAfterCommitEventHandler<in TEvent>
    where TEvent : DomainEvent
{
    Task HandleAsync(TEvent domainEvent, EventContext context, CancellationToken cancellationToken);
}
