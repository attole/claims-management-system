using ClaimsModule.Domain.Claims.Root.Bases;
using ClaimsModule.Domain.Common.Roles;

namespace ClaimsModule.Domain.Tests.Support.Claims;

public sealed record SampleClaimTransitionInput(UserRole ActorRole, Guid ActorId, DateTimeOffset Now)
    : ClaimBaseTransitionInput(ActorRole, ActorId, Now);
