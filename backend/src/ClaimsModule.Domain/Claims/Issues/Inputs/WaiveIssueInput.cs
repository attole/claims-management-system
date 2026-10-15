using ClaimsModule.Domain.Claims.Root.Inputs;

namespace ClaimsModule.Domain.Claims.Issues.Inputs;

public sealed record WaiveIssueInput(Guid IssueId, Guid ActorId, DateTimeOffset Now, ClaimReferenceSnapshot Reference)
    : ClaimBaseOperationInput(Now, ActorId, Reference);
