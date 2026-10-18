using ClaimsModule.Domain.ReserveComponents.History.Rules;

namespace ClaimsModule.Domain.ReserveComponents.History.Flows;

internal sealed class RetryHistoryPostingTransition
    : ReserveHistoryBasePostingTransition<RetryHistoryPostingTransition>
{
    public RetryHistoryPostingTransition() : base(new ReservePostingPendingState())
    {
        AllowedFrom(new ReservePostingFailedState())
            .Require(input => new HasPostingJob(input.JobId));
    }
}
