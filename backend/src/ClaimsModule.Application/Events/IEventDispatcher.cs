namespace ClaimsModule.Application.Events;

public interface IEventDispatcher
{
    Task DispatchBeforeCommitAsync(IReadOnlyList<EventContext> contexts, CancellationToken cancellationToken);

    Task DispatchAfterCommitAsync(IReadOnlyList<EventContext> contexts, CancellationToken cancellationToken);
}
