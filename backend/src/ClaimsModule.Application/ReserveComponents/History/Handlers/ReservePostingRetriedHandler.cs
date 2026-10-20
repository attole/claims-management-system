using ClaimsModule.Application.Abstractions;
using ClaimsModule.Application.Events.Contracts;
using ClaimsModule.Domain.ReserveComponents.History;
using ClaimsModule.Domain.ReserveComponents.History.Events;

namespace ClaimsModule.Application.ReserveComponents.History.Handlers;

public sealed class ReservePostingRetriedHandler(IPostingScheduler scheduler)
    : IAfterCommitEventHandler<ReserveHistoryPostingStateChangedEvent>
{
    public async Task HandleAsync(
        ReserveHistoryPostingStateChangedEvent domainEvent,
        ExtendedEvent extendedEvent,
        CancellationToken cancellationToken)
    {
        var from = Enum.Parse<ReservePostingStateKind>(domainEvent.From);
        var to = Enum.Parse<ReservePostingStateKind>(domainEvent.To);
        bool isRetried = from == ReservePostingStateKind.Failed && to == ReservePostingStateKind.Pending;

        if (isRetried)
        {
            await scheduler.EnqueueAsync(
                domainEvent.ClaimId,
                domainEvent.ReserveHistoryId,
                domainEvent.JobId,
                cancellationToken);
        }
    }
}
