using ClaimsModule.Domain.Claims.Root.Bases;
using ClaimsModule.Domain.Claims.Root.Rules;
using ClaimsModule.Domain.Core.Operations;

namespace ClaimsModule.Domain.Claims.Root.Operations;

public sealed record AssignHandlerInput(Guid HandlerId, Guid ActorId, DateTimeOffset Now)
    : ClaimBaseOperationInput(Now, ActorId);

public sealed class AssignHandlerOperation : ClaimBaseOperation<AssignHandlerInput, Success>
{
    public AssignHandlerOperation()
    {
        Require(new IsClaimEditable());
    }

    protected override Success Apply(Claim claim, AssignHandlerInput input)
    {
        claim.AssignHandler(input.HandlerId, input.Now);

        return Success.Value;
    }
}
