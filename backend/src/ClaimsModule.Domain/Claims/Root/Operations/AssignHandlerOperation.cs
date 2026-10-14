using ClaimsModule.Domain.Core.Operations;

namespace ClaimsModule.Domain.Claims.Root.Operations;

public sealed record AssignHandlerInput(Guid HandlerId, Guid ActorId, DateTimeOffset Now)
    : ClaimBaseInput(Now, ActorId);

public sealed class AssignHandlerOperation : ClaimBaseOperation<AssignHandlerInput, Success>
{
    protected override Success Apply(Claim claim, AssignHandlerInput input)
    {
        claim.AssignHandler(input.HandlerId, input.Now);

        return Success.Value;
    }
}
