using ClaimsModule.Domain.Common;
using ClaimsModule.Domain.Core.Operations;
using ClaimsModule.Domain.ReserveChanges.Root.Inputs;
using ClaimsModule.Domain.ReserveChanges.Root.Rules;

namespace ClaimsModule.Domain.ReserveChanges.Root.Flows;

internal sealed class SubmitReserveOperation
    : CreationOperation<SubmitReserveOperation, SubmitReserveInput, ReserveChange>
{
    public SubmitReserveOperation()
    {
        Require(input => new IsReserveAmountNonZero(input.Amount));
        Require(input => new IsPolicyLinked(input.Snapshot.IsPolicyLinked));
        Require(input => new HasNonNegativeBalance(input.Snapshot, input.Amount));
        Require(input => new IsClaimTotalWithinLimit(input.Snapshot, input.Amount));
    }

    protected override ReserveChange Apply(SubmitReserveInput input)
    {
        var details = new ReserveChangeDetails(
            input.ReserveId,
            input.ClaimId,
            input.TransactionType,
            input.Amount,
            input.ChangeReason,
            input.Snapshot.NextChangeSequence);

        return new ReserveChange(Ids.Next(input.Now), input.OrganisationId, details, input.ActorId, input.Now);
    }
}
