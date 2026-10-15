using ClaimsModule.Domain.Claims.Root.Inputs;
using ClaimsModule.Domain.Claims.Root.Rules;
using ClaimsModule.Domain.Core.Operations;

namespace ClaimsModule.Domain.Claims.Root.Flows;

internal sealed class AssignHandlerOperation : Operation<AssignHandlerOperation, Claim, AssignHandlerInput, Success>
{
    public AssignHandlerOperation()
    {
        Require(new IsClaimEditable());
    }

    protected override Success Apply(Claim claim, AssignHandlerInput input)
    {
        claim.AssignedHandlerId = input.HandlerId;

        return Success.Value;
    }
}
