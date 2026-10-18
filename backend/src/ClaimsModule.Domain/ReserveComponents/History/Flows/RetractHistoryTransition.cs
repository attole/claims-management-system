using ClaimsModule.Domain.ReserveComponents.History.Inputs;
using ClaimsModule.Domain.ReserveComponents.History.Rules;

namespace ClaimsModule.Domain.ReserveComponents.History.Flows;

internal sealed class RetractHistoryTransition
    : ReserveHistoryBaseTransition<RetractHistoryTransition, RowDecisionInput>
{
    public RetractHistoryTransition() : base(new ReserveCancelledState())
    {
        AllowedFrom(new ReservePendingApprovalState())
            .Require(input => new IsSubmitter(input.ActorId));
    }
}
