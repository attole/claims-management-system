using ClaimsModule.Domain.Core.Flows;
using ClaimsModule.Domain.ReserveComponents.History.Inputs;

namespace ClaimsModule.Domain.ReserveComponents.History.Flows;

internal sealed class RejectHistoryOnReserveComponentOperation
    : Operation<
        RejectHistoryOnReserveComponentOperation,
        ReserveComponent,
        RejectHistoryInput,
        Move<ReserveApprovalState>>
{
    protected override Outcome<Move<ReserveApprovalState>> Apply(ReserveComponent component, RejectHistoryInput input)
    {
        var history = component.History.Single(history => history.Id == input.ReserveHistoryId);
        var rejection = new RejectRowInput(input.ActorRole, input.ActorId, input.Now)
        {
            Reason = input.Reason,
        };

        return history.Reject(rejection);
    }
}
