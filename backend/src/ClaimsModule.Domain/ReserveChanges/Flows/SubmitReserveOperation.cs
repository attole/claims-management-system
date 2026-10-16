using ClaimsModule.Domain.Common;
using ClaimsModule.Domain.Core.Flows;
using ClaimsModule.Domain.ReserveChanges.Inputs;
using ClaimsModule.Domain.ReserveChanges.Rules;
using ClaimsModule.Domain.Reserves.Rules;

namespace ClaimsModule.Domain.ReserveChanges.Flows;

internal sealed class SubmitReserveOperation
    : CreationOperation<SubmitReserveOperation, SubmitReserveInput, ReserveChange>
{
    public SubmitReserveOperation()
    {
        Require(input => new IsClaimAcceptingReserves(input.IsClaimEditable));
        Require(input => new IsReserveAmountNonZero(input.Amount));
        Require(input => new IsPolicyLinked(input.IsPolicyLinked));
        Require(input => new HasNonNegativeBalance(input.ReserveBalanceAfter, input.Reserve.ComponentType));
        Require(input => new IsClaimTotalWithinLimit(input.ClaimApprovedTotal, input.Amount));
    }

    protected override Outcome<ReserveChange> Apply(SubmitReserveInput input)
    {
        var details = new ReserveChangeDetails(
            input.Reserve.ReserveId,
            input.Reserve.ClaimId,
            input.Reserve.ComponentType,
            input.TransactionType,
            input.Amount,
            input.ChangeReason,
            input.ChangeSequence);

        var organisationId = input.Reserve.OrganisationId;
        var change = new ReserveChange(Ids.Next(), organisationId, details, input.ActorId, input.Now);

        return Done(change);
    }
}
