using ClaimsModule.Domain.ReserveChanges.Rules;

namespace ClaimsModule.Domain.ReserveChanges.Flows;

internal sealed class MarkReservePostedTransition
    : ReserveChangeBasePostingTransition<MarkReservePostedTransition>
{
    public MarkReservePostedTransition() : base(new ReservePostingPostedState())
    {
        AllowedFrom(new ReservePostingPendingState())
            .Require(new IsReserveChangeApproved())
            .Require(input => new HasPostingJob(input.JobId));
    }
}
