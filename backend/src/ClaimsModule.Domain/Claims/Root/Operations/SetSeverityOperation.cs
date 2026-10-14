using ClaimsModule.Domain.Claims.Root.Bases;
using ClaimsModule.Domain.Claims.Root.Rules;
using ClaimsModule.Domain.Core.Operations;

namespace ClaimsModule.Domain.Claims.Root.Operations;

public sealed record SetSeverityInput(ClaimSeverity Severity, Guid ActorId, DateTimeOffset Now)
    : ClaimBaseOperationInput(Now, ActorId);

public sealed class SetSeverityOperation : ClaimBaseOperation<SetSeverityInput, Success>
{
    public SetSeverityOperation()
    {
        Require(new IsClaimEditable());
    }

    protected override Success Apply(Claim claim, SetSeverityInput input)
    {
        claim.SetSeverity(input.Severity, input.Now);

        return Success.Value;
    }
}
