using ClaimsModule.Domain.Claims;
using ClaimsModule.Domain.Common;

namespace ClaimsModule.Domain.ReserveComponents.History.Inputs;

public sealed record SubmitHistoryInput(
    ReserveTransactionType TransactionType,
    decimal Amount,
    string ChangeReason,
    Guid ActorId,
    ClaimState ClaimState,
    bool IsPolicyLinked,
    decimal ClaimApprovedTotal,
    DateTimeOffset Now)
{
    public Guid ActorId { get; } = InputActor.Required(ActorId);

    public DateTimeOffset Now { get; } = InputTime.Required(Now);
}
