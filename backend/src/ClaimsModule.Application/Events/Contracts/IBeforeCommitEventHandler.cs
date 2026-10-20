using ClaimsModule.Domain.Common.Events;

namespace ClaimsModule.Application.Events.Contracts;

public interface IBeforeCommitEventHandler
{
    Type EventType { get; }

    Task HandleAsync(ExtendedEvent extendedEvent, CancellationToken cancellationToken);
}

public interface IBeforeCommitEventHandler<in TEvent> : IBeforeCommitEventHandler
    where TEvent : DomainEvent
{
    Type IBeforeCommitEventHandler.EventType => typeof(TEvent);

    Task IBeforeCommitEventHandler.HandleAsync(ExtendedEvent extendedEvent, CancellationToken cancellationToken)
    {
        return HandleAsync((TEvent)extendedEvent.Event, extendedEvent, cancellationToken);
    }

    Task HandleAsync(TEvent domainEvent, ExtendedEvent extendedEvent, CancellationToken cancellationToken);
}
