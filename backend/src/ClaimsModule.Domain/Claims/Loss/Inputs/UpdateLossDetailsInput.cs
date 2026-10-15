using ClaimsModule.Domain.Claims.Issues;
using ClaimsModule.Domain.Claims.Root.Inputs;

namespace ClaimsModule.Domain.Claims.Loss.Inputs;

public sealed record UpdateLossDetailsInput(
    LossDetails Loss,
    Guid ActorId,
    DateTimeOffset Now,
    ClaimReferenceSnapshot Reference) : ClaimBaseOperationInput(Now, ActorId, Reference);
