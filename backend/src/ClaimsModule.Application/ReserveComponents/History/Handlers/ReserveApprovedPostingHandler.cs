using ClaimsModule.Application.Abstractions;
using ClaimsModule.Application.Events.Contracts;
using ClaimsModule.Domain.ReserveComponents.History;
using ClaimsModule.Domain.ReserveComponents.History.Events;

namespace ClaimsModule.Application.ReserveComponents.History.Handlers;

public sealed class ReserveApprovedPostingHandler(IPostingScheduler scheduler, ICurrentUser user)
    : IAfterCommitEventHandler<ReserveHistoryApprovalStateChangedEvent>
{
    public async Task HandleAsync(
        ReserveHistoryApprovalStateChangedEvent domainEvent,
        ExtendedEvent extendedEvent,
        CancellationToken cancellationToken)
    {
        var target = ReserveApprovalState.From(Enum.Parse<ReserveApprovalStateKind>(domainEvent.To));

        if (target.IsApproved)
        {
            await scheduler.EnqueueAsync(
                user.OrganisationId,
                domainEvent.ClaimId,
                domainEvent.ReserveHistoryId,
                string.Empty,
                cancellationToken);
        }
    }
}
