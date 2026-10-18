using ClaimsModule.Domain.Core.Flows;
using ClaimsModule.Domain.ReserveComponents.History.Inputs;

namespace ClaimsModule.Domain.ReserveComponents.History.Flows;

internal sealed class RetractHistoryOnReserveComponentOperation
    : Operation<
        RetractHistoryOnReserveComponentOperation,
        ReserveComponent,
        RetractHistoryInput,
        Move<ReserveApprovalState>>
{
    protected override Outcome<Move<ReserveApprovalState>> Apply(ReserveComponent component, RetractHistoryInput input)
    {
        var history = component.History.Single(history => history.Id == input.ReserveHistoryId);
        var retraction = new RowDecisionInput(input.ActorId, input.Now)
        {
            Reason = input.Reason,
        };

        return history.Retract(retraction);
    }
}
