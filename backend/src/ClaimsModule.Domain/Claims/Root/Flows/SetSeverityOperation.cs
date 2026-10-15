using ClaimsModule.Domain.Claims.Root.Inputs;
using ClaimsModule.Domain.Claims.Root.Rules;
using ClaimsModule.Domain.Core.Operations;

namespace ClaimsModule.Domain.Claims.Root.Flows;

internal sealed class SetSeverityOperation : Operation<SetSeverityOperation, Claim, SetSeverityInput, Success>
{
    public SetSeverityOperation()
    {
        Require(new IsClaimEditable());
    }

    protected override Success Apply(Claim claim, SetSeverityInput input)
    {
        claim.Severity = input.Severity;

        return Success.Value;
    }
}
