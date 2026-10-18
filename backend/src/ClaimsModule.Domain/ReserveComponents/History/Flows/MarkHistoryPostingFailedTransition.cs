using ClaimsModule.Domain.ReserveComponents.History.Rules;

namespace ClaimsModule.Domain.ReserveComponents.History.Flows;

internal sealed class MarkHistoryPostingFailedTransition
    : ReserveHistoryBasePostingTransition<MarkHistoryPostingFailedTransition>
{
    public MarkHistoryPostingFailedTransition() : base(new ReservePostingFailedState())
    {
        AllowedFrom(new ReservePostingPendingState())
            .Require(new IsReserveHistoryApproved())
            .Require(input => new HasPostingJob(input.JobId));
    }
}
