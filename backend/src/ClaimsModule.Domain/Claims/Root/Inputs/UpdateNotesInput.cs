using ClaimsModule.Domain.Claims.Issues;

namespace ClaimsModule.Domain.Claims.Root.Inputs;

public sealed record UpdateNotesInput(string Notes, Guid ActorId, DateTimeOffset Now, ClaimReferenceSnapshot Reference)
    : ClaimBaseOperationInput(Now, ActorId, Reference);
