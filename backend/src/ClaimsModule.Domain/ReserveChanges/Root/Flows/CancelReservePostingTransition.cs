using ClaimsModule.Domain.ReserveChanges.Root.Bases;
using ClaimsModule.Domain.ReserveChanges.Root.Inputs;
using ClaimsModule.Domain.ReserveChanges.Root.Rules;

namespace ClaimsModule.Domain.ReserveChanges.Root.Flows;

internal sealed class CancelReservePostingTransition
    : ReserveChangeBasePostingTransition<CancelReservePostingTransition, CancelReservePostingInput>
{
    public CancelReservePostingTransition() : base(new ReservePostingCancelledState())
    {
        AllowedFrom(new ReservePostingPendingState())
            .Require(new IsReserveChangeRefused());
    }
}
