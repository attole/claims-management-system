using ClaimsModule.Domain.ReserveComponents.History.Rules;

namespace ClaimsModule.Domain.ReserveComponents.History.Flows;

internal sealed class MarkHistoryPostedTransition
    : ReserveHistoryBasePostingTransition<MarkHistoryPostedTransition>
{
    public MarkHistoryPostedTransition() : base(new ReservePostingPostedState())
    {
        AllowedFrom(new ReservePostingPendingState())
            .Require(new IsReserveHistoryApproved())
            .Require(input => new HasPostingJob(input.JobId));
    }
}
