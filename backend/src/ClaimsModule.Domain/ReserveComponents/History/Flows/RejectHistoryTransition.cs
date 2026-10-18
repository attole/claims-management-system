using ClaimsModule.Domain.ReserveComponents.History.Inputs;
using ClaimsModule.Domain.ReserveComponents.History.Rules;

namespace ClaimsModule.Domain.ReserveComponents.History.Flows;

internal sealed class RejectHistoryTransition
    : ReserveHistoryBaseTransition<RejectHistoryTransition, RejectRowInput>
{
    public RejectHistoryTransition() : base(new ReserveRejectedState())
    {
        AllowedFrom(new ReservePendingApprovalState())
            .Require(input => new HasRejectionAuthority(input.ActorRole))
            .Require(input => new HasRejectionReason(input.Reason));
    }
}
