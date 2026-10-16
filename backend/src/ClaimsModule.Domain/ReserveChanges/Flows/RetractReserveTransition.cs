using ClaimsModule.Domain.ReserveChanges.Inputs;
using ClaimsModule.Domain.ReserveChanges.Rules;

namespace ClaimsModule.Domain.ReserveChanges.Flows;

internal sealed class RetractReserveTransition
    : ReserveChangeBaseTransition<RetractReserveTransition, ReserveDecisionInput>
{
    public RetractReserveTransition() : base(new ReserveCancelledState())
    {
        AllowedFrom(new ReservePendingApprovalState())
            .Require(input => new IsSubmitter(input.ActorId));
    }
}
