using ClaimsModule.Domain.ReserveChanges.Inputs;
using ClaimsModule.Domain.ReserveChanges.Rules;

namespace ClaimsModule.Domain.ReserveChanges.Flows;

internal sealed class RejectReserveTransition
    : ReserveChangeBaseTransition<RejectReserveTransition, RejectReserveInput>
{
    public RejectReserveTransition() : base(new ReserveRejectedState())
    {
        AllowedFrom(new ReservePendingApprovalState())
            .Require(input => new HasRejectionAuthority(input.ActorRole))
            .Require(input => new HasRejectionReason(input.Reason));
    }
}
