using ClaimsModule.Domain.Claims.Root.Inputs;

namespace ClaimsModule.Domain.Claims.Issues.Inputs;

public sealed record EvaluateIssuesInput(DateTimeOffset Now, Guid ActorId, ClaimReferenceSnapshot Reference)
    : ClaimBaseOperationInput(Now, ActorId, Reference);
