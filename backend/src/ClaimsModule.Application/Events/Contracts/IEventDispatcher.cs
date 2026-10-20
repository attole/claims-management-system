namespace ClaimsModule.Application.Events.Contracts;

public interface IEventDispatcher
{
    Task DispatchBeforeCommitAsync(IReadOnlyList<ExtendedEvent> extendedEvents, CancellationToken cancellationToken);

    Task DispatchAfterCommitAsync(IReadOnlyList<ExtendedEvent> extendedEvents, CancellationToken cancellationToken);
}
