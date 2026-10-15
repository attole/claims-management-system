using ClaimsModule.Domain.Claims.Issues;
using ClaimsModule.Domain.Claims.Root.Inputs;
using ClaimsModule.Domain.Common.Roles;

namespace ClaimsModule.Domain.Tests.Support.Claims;

public sealed record SampleClaimTransitionInput(
    UserRole ActorRole,
    Guid ActorId,
    DateTimeOffset Now,
    ClaimReferenceSnapshot Reference) : ClaimBaseTransitionInput(ActorRole, ActorId, Now, Reference);
