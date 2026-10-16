using ClaimsModule.Domain.ReserveChanges.Rules;

namespace ClaimsModule.Domain.ReserveChanges.Flows;

internal sealed class RetryReservePostingTransition
    : ReserveChangeBasePostingTransition<RetryReservePostingTransition>
{
    public RetryReservePostingTransition() : base(new ReservePostingPendingState())
    {
        AllowedFrom(new ReservePostingFailedState())
            .Require(input => new HasPostingJob(input.JobId));
    }
}
