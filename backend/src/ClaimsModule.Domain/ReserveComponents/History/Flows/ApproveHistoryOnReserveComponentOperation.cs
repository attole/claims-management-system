using ClaimsModule.Domain.Core.Flows;
using ClaimsModule.Domain.ReserveComponents.History.Inputs;

namespace ClaimsModule.Domain.ReserveComponents.History.Flows;

internal sealed class ApproveHistoryOnReserveComponentOperation
    : Operation<
        ApproveHistoryOnReserveComponentOperation,
        ReserveComponent,
        ApproveHistoryInput,
        Move<ReserveApprovalState>>
{
    protected override Outcome<Move<ReserveApprovalState>> Apply(ReserveComponent component, ApproveHistoryInput input)
    {
        var history = component.History.Single(history => history.Id == input.ReserveHistoryId);
        var approval = new ApproveRowInput(
            input.ClaimApprovedTotal,
            component.ApprovedBalance,
            input.HasAggregateLimitOverride,
            input.ActorRole,
            input.ActorId,
            input.Now)
        {
            Reason = input.Reason,
        };

        return history.Approve(approval);
    }
}
