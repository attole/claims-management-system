using ClaimsModule.Domain.Common;
using ClaimsModule.Domain.Core.Flows;
using ClaimsModule.Domain.ReserveComponents.History.Inputs;
using ClaimsModule.Domain.ReserveComponents.History.Rules;
using ClaimsModule.Domain.ReserveComponents.Rules;

namespace ClaimsModule.Domain.ReserveComponents.History.Flows;

internal sealed class SubmitHistoryOperation
    : Operation<SubmitHistoryOperation, ReserveComponent, SubmitHistoryInput, ReserveHistory>
{
    public SubmitHistoryOperation()
    {
        Require(input => new IsClaimAcceptingReserves(input.ClaimState));
        Require(new IsReserveComponentActive());
        Require(input => new IsReserveAmountNonZero(input.Amount));
        Require(input => new IsPolicyLinked(input.IsPolicyLinked));
        Require(input => new HasNonNegativeBalance(input.Amount));
        Require(input => new IsClaimTotalWithinLimit(input.ClaimApprovedTotal, input.Amount));
    }

    protected override Outcome<ReserveHistory> Apply(ReserveComponent component, SubmitHistoryInput input)
    {
        var details = new ReserveHistoryDetails(
            component.ClaimId,
            component.ComponentType,
            input.TransactionType,
            input.Amount,
            input.ChangeReason,
            component.NextChangeSequence);

        var history = new ReserveHistory(
            Ids.Next(),
            component.Id,
            component.OrganisationId,
            details,
            input.ActorId,
            input.Now);
        component.AttachHistory(history);

        return Done(history);
    }
}
