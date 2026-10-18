using ClaimsModule.Domain.ReserveComponents.History.Rules;

namespace ClaimsModule.Domain.ReserveComponents.History.Flows;

internal sealed class CancelHistoryPostingTransition
    : ReserveHistoryBasePostingTransition<CancelHistoryPostingTransition>
{
    public CancelHistoryPostingTransition() : base(new ReservePostingCancelledState())
    {
        AllowedFrom(new ReservePostingPendingState())
            .Require(new IsReserveHistoryRefused());
    }
}
