using ClaimsModule.Domain.Common;
using ClaimsModule.Domain.Reserves;

namespace ClaimsModule.Domain.ReserveChanges.Inputs;

public sealed record SubmitReserveInput(
    ReserveReference Reserve,
    ReserveTransactionType TransactionType,
    decimal Amount,
    string ChangeReason,
    Guid ActorId,
    bool IsClaimEditable,
    bool IsPolicyLinked,
    decimal ClaimApprovedTotal,
    decimal ReserveApprovedBalance,
    int ChangeSequence,
    DateTimeOffset Now)
{
    private const string SequenceMessage = "A reserve change needs a positive change sequence.";

    public Guid ActorId { get; } = InputActor.Required(ActorId);

    public int ChangeSequence { get; } = RequiredSequence(ChangeSequence);

    public DateTimeOffset Now { get; } = InputTime.Required(Now);

    public decimal ReserveBalanceAfter => ReserveApprovedBalance + Amount;

    private static int RequiredSequence(int changeSequence)
    {
        bool isPositive = changeSequence > 0;

        return isPositive
            ? changeSequence
            : throw new ArgumentOutOfRangeException(nameof(changeSequence), SequenceMessage);
    }
}
