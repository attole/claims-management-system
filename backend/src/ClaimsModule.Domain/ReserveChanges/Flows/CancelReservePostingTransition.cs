using ClaimsModule.Domain.ReserveChanges.Rules;

namespace ClaimsModule.Domain.ReserveChanges.Flows;

internal sealed class CancelReservePostingTransition
    : ReserveChangeBasePostingTransition<CancelReservePostingTransition>
{
    public CancelReservePostingTransition() : base(new ReservePostingCancelledState())
    {
        AllowedFrom(new ReservePostingPendingState())
            .Require(new IsReserveChangeRefused());
    }
}
