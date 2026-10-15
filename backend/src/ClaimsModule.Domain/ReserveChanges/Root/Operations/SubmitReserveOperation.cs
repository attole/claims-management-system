using ClaimsModule.Domain.Core.Operations;
using ClaimsModule.Domain.ReserveChanges.Root.Bases;
using ClaimsModule.Domain.ReserveChanges.Root.Rules;

namespace ClaimsModule.Domain.ReserveChanges.Root.Operations;

public sealed record SubmitReserveInput(
    Guid OrganisationId,
    Guid ReserveId,
    Guid ClaimId,
    ReserveTransactionType TransactionType,
    decimal Amount,
    string ChangeReason,
    Guid ActorId,
    ReserveSnapshot Snapshot,
    DateTimeOffset Now) : ReserveChangeBaseOperationInput(Now, ActorId);

public sealed class SubmitReserveOperation : CreationOperation<SubmitReserveInput, ReserveChange>
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

        return ReserveChange.Submit(input.OrganisationId, details, input.ActorId, input.Now);
    }
}
