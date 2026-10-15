using ClaimsModule.Domain.ReserveChanges.Root.Bases;
using ClaimsModule.Domain.ReserveChanges.Root.Inputs;
using ClaimsModule.Domain.ReserveChanges.Root.Rules;

namespace ClaimsModule.Domain.ReserveChanges.Root.Flows;

internal sealed class MarkReservePostedTransition
    : ReserveChangeBasePostingTransition<MarkReservePostedTransition, MarkReservePostedInput>
{
    public MarkReservePostedTransition() : base(new ReservePostingPostedState())
    {
        AllowedFrom(new ReservePostingPendingState(), new ReservePostingFailedState())
            .Require(new IsReserveChangeApproved());
    }
}
