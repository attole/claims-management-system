using ClaimsModule.Domain.ReserveChanges.Root.Bases;
using ClaimsModule.Domain.ReserveChanges.Root.Inputs;
using ClaimsModule.Domain.ReserveChanges.Root.Rules;

namespace ClaimsModule.Domain.ReserveChanges.Root.Flows;

internal sealed class RejectReserveTransition : ReserveChangeBaseTransition<RejectReserveTransition, RejectReserveInput>
{
    public RejectReserveTransition() : base(new ReserveRejectedState())
    {
        AllowedFrom(new ReservePendingApprovalState())
            .Require(input => new HasApprovalRole(input.ActorRole))
            .Require(input => new HasRejectionReason(input.Reason));
    }
}
