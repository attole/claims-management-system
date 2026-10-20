using ClaimsModule.Application.Events.Contracts;

namespace ClaimsModule.Application.Events.Services;

public sealed class EventDispatcher(
    IEnumerable<IBeforeCommitEventHandler> beforeCommitHandlers,
    IEnumerable<IAfterCommitEventHandler> afterCommitHandlers) : IEventDispatcher
{
    public async Task DispatchBeforeCommitAsync(
        IReadOnlyList<ExtendedEvent> extendedEvents,
        CancellationToken cancellationToken)
    {
        foreach (var extendedEvent in extendedEvents)
        {
            var eventType = extendedEvent.Event.GetType();
            var handlers = beforeCommitHandlers.Where(handler => handler.EventType == eventType);

            foreach (var handler in handlers)
            {
                await handler.HandleAsync(extendedEvent, cancellationToken);
            }
        }
    }

    public async Task DispatchAfterCommitAsync(
        IReadOnlyList<ExtendedEvent> extendedEvents,
        CancellationToken cancellationToken)
    {
        foreach (var extendedEvent in extendedEvents)
        {
            var eventType = extendedEvent.Event.GetType();
            var handlers = afterCommitHandlers.Where(handler => handler.EventType == eventType);

            foreach (var handler in handlers)
            {
                await handler.HandleAsync(extendedEvent, cancellationToken);
            }
        }
    }
}
