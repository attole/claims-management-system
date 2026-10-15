using ClaimsModule.Domain.ReserveChanges.Root.Bases;
using ClaimsModule.Domain.ReserveChanges.Root.Inputs;
using ClaimsModule.Domain.ReserveChanges.Root.Rules;

namespace ClaimsModule.Domain.ReserveChanges.Root.Flows;

internal sealed class MarkReservePostingFailedTransition
    : ReserveChangeBasePostingTransition<MarkReservePostingFailedTransition, MarkReservePostingFailedInput>
{
    public MarkReservePostingFailedTransition() : base(new ReservePostingFailedState())
    {
        AllowedFrom(new ReservePostingPendingState())
            .Require(new IsReserveChangeApproved());
    }
}
