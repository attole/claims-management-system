namespace ClaimsModule.Application.Events.Contracts;

public interface IEventDispatcher
{
    Task DispatchBeforeCommitAsync(IReadOnlyList<EventContext> contexts, CancellationToken cancellationToken);

    Task DispatchAfterCommitAsync(IReadOnlyList<EventContext> contexts, CancellationToken cancellationToken);
}
