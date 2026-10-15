using ClaimsModule.Domain.Claims.Issues;

namespace ClaimsModule.Domain.Claims.Root.Inputs;

public sealed record AssignHandlerInput(
    Guid HandlerId,
    Guid ActorId,
    DateTimeOffset Now,
    ClaimReferenceSnapshot Reference) : ClaimBaseOperationInput(Now, ActorId, Reference);
