using ClaimsModule.Domain.Claims.Issues;

namespace ClaimsModule.Domain.Claims.Root.Inputs;

public sealed record LinkPolicyInput(Guid PolicyId, Guid ActorId, DateTimeOffset Now, ClaimReferenceSnapshot Reference)
    : ClaimBaseOperationInput(Now, ActorId, Reference);
