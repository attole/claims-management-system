using ClaimsModule.Domain.ReserveChanges.Root.Bases;
using ClaimsModule.Domain.ReserveChanges.Root.Inputs;
using ClaimsModule.Domain.ReserveChanges.Root.Rules;

namespace ClaimsModule.Domain.ReserveChanges.Root.Flows;

internal sealed class RetractReserveTransition
    : ReserveChangeBaseTransition<RetractReserveTransition, RetractReserveInput>
{
    public RetractReserveTransition() : base(new ReserveCancelledState())
    {
        AllowedFrom(new ReservePendingApprovalState())
            .Require(input => new IsSubmitter(input.ActorId));
    }
}
