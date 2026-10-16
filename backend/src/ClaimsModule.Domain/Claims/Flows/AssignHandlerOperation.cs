using ClaimsModule.Domain.Claims.Inputs;
using ClaimsModule.Domain.Claims.Rules;
using ClaimsModule.Domain.Core.Flows;

namespace ClaimsModule.Domain.Claims.Flows;

internal sealed class AssignHandlerOperation : Operation<AssignHandlerOperation, Claim, AssignHandlerInput, Claim>
{
    public AssignHandlerOperation()
    {
        Require(new IsClaimEditable());
        Require(input => new IsHandlerKeptAfterDraft(input.HandlerId));
    }

    protected override Outcome<Claim> Apply(Claim claim, AssignHandlerInput input)
    {
        claim.AssignedHandlerId = input.HandlerId;

        return Done(claim);
    }
}
