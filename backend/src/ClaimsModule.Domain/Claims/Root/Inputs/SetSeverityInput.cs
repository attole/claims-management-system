using ClaimsModule.Domain.Claims.Issues;

namespace ClaimsModule.Domain.Claims.Root.Inputs;

public sealed record SetSeverityInput(
    ClaimSeverity Severity,
    Guid ActorId,
    DateTimeOffset Now,
    ClaimReferenceSnapshot Reference) : ClaimBaseOperationInput(Now, ActorId, Reference);
