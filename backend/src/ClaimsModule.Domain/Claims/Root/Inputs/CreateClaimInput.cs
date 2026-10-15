using ClaimsModule.Domain.Claims.Issues;

namespace ClaimsModule.Domain.Claims.Root.Inputs;

public sealed record CreateClaimInput(
    Guid OrganisationId,
    ClaimNumber ClaimNumber,
    Guid ActorId,
    DateTimeOffset Now,
    ClaimReferenceSnapshot Reference) : ClaimBaseOperationInput(Now, ActorId, Reference);
