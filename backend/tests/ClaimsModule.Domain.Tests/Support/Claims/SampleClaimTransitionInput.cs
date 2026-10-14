using ClaimsModule.Domain.Claims.Root;

namespace ClaimsModule.Domain.Tests.Support.Claims;

public sealed record SampleClaimTransitionInput(Guid ActorId, DateTimeOffset Now) : ClaimBaseInput(Now, ActorId);
