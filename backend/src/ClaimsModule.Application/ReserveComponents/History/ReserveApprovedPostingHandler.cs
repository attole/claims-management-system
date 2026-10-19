using ClaimsModule.Application.Events;
using ClaimsModule.Application.Jobs;
using ClaimsModule.Domain.ReserveComponents.History;
using ClaimsModule.Domain.ReserveComponents.History.Events;

namespace ClaimsModule.Application.ReserveComponents.History;

public sealed class ReserveApprovedPostingHandler(IPostingScheduler scheduler)
    : IAfterCommitEventHandler<ReserveHistoryApprovalStateChangedEvent>
{
    public async Task HandleAsync(
        ReserveHistoryApprovalStateChangedEvent domainEvent,
        EventContext context,
        CancellationToken cancellationToken)
    {
        var target = ReserveApprovalState.From(Enum.Parse<ReserveApprovalStateKind>(domainEvent.To));

        if (target.IsApproved)
        {
            await scheduler.EnqueueAsync(
                domainEvent.ClaimId,
                domainEvent.ReserveHistoryId,
                string.Empty,
                cancellationToken);
        }
    }
}
