using ClaimsModule.Domain.Common;
using ClaimsModule.Domain.Engine.Operations;

namespace ClaimsModule.Domain.Claims.Root.Operations;

public sealed record SetSeverityInput(ClaimSeverity Severity, DateTimeOffset Now) : OperationInput(Now);

public sealed class SetSeverityOperation : Operation<Claim, SetSeverityInput, Success>
{
    protected override Success Apply(Claim claim, SetSeverityInput input)
    {
        claim.SetSeverity(input.Severity, input.Now);

        return Success.Value;
    }
}
