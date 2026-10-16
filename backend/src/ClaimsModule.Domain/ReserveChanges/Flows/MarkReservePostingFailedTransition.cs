using ClaimsModule.Domain.ReserveChanges.Rules;

namespace ClaimsModule.Domain.ReserveChanges.Flows;

internal sealed class MarkReservePostingFailedTransition
    : ReserveChangeBasePostingTransition<MarkReservePostingFailedTransition>
{
    public MarkReservePostingFailedTransition() : base(new ReservePostingFailedState())
    {
        AllowedFrom(new ReservePostingPendingState())
            .Require(new IsReserveChangeApproved())
            .Require(input => new HasPostingJob(input.JobId));
    }
}
