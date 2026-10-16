using ClaimsModule.Domain.Claims.Inputs;
using ClaimsModule.Domain.Claims.Rules;
using ClaimsModule.Domain.Core.Flows;

namespace ClaimsModule.Domain.Claims.Flows;

internal sealed class SetSeverityOperation : Operation<SetSeverityOperation, Claim, SetSeverityInput, Claim>
{
    public SetSeverityOperation()
    {
        Require(new IsClaimEditable());
    }

    protected override Outcome<Claim> Apply(Claim claim, SetSeverityInput input)
    {
        claim.Severity = input.Severity;

        return Done(claim);
    }
}
