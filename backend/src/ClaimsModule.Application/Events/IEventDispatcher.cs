namespace ClaimsModule.Application.Events;

public interface IEventDispatcher
{
    Task DispatchInTransactionAsync(IReadOnlyList<EventContext> contexts, CancellationToken cancellationToken);

    Task DispatchAfterTransactionAsync(IReadOnlyList<EventContext> contexts, CancellationToken cancellationToken);
}
