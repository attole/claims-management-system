using ClaimsModule.Domain.Claims.Issues;
using ClaimsModule.Domain.Claims.Root.Inputs;

namespace ClaimsModule.Domain.Claims.RiskObjects.Inputs;

public sealed record RemoveRiskObjectInput(
    Guid RiskObjectId,
    Guid ActorId,
    DateTimeOffset Now,
    ClaimReferenceSnapshot Reference) : ClaimBaseOperationInput(Now, ActorId, Reference);
