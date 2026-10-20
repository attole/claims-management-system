using ClaimsModule.Domain.Common.Events;

namespace ClaimsModule.Application.Events.Contracts;

public interface IAfterCommitEventHandler
{
    Type EventType { get; }

    Task HandleAsync(ExtendedEvent extendedEvent, CancellationToken cancellationToken);
}

public interface IAfterCommitEventHandler<in TEvent> : IAfterCommitEventHandler
    where TEvent : DomainEvent
{
    Type IAfterCommitEventHandler.EventType => typeof(TEvent);

    Task IAfterCommitEventHandler.HandleAsync(ExtendedEvent extendedEvent, CancellationToken cancellationToken)
    {
        return HandleAsync((TEvent)extendedEvent.Event, extendedEvent, cancellationToken);
    }

    Task HandleAsync(TEvent domainEvent, ExtendedEvent extendedEvent, CancellationToken cancellationToken);
}
