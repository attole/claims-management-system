using ClaimsModule.Domain.Common.Roles;

namespace ClaimsModule.Domain.ReserveChanges.Root.Inputs;

public sealed record SubmitReserveInput(
    Guid OrganisationId,
    Guid ReserveId,
    Guid ClaimId,
    ReserveTransactionType TransactionType,
    decimal Amount,
    string ChangeReason,
    Guid ActorId,
    UserRole ActorRole,
    ReserveSnapshot Snapshot,
    DateTimeOffset Now) : ReserveChangeBaseOperationInput(Now, ActorId);
