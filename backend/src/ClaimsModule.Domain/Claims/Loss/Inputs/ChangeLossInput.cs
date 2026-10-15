using ClaimsModule.Domain.Claims.Issues;
using ClaimsModule.Domain.Claims.Root;
using ClaimsModule.Domain.Core.Operations;

namespace ClaimsModule.Domain.Claims.Loss.Inputs;

internal sealed record ChangeLossInput(
    LossDetails Details,
    ClaimStateKind ClaimState,
    ClaimReferenceSnapshot Reference,
    DateTimeOffset Now,
    Guid ActorId) : OperationInput(Now, ActorId);
