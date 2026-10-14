using ClaimsModule.Domain.Core.Operations;

namespace ClaimsModule.Domain.Claims.Root.Operations;

public sealed record SetSeverityInput(ClaimSeverity Severity, Guid ActorId, DateTimeOffset Now)
    : ClaimBaseInput(Now, ActorId);

public sealed class SetSeverityOperation : ClaimBaseOperation<SetSeverityInput, Success>
{
    protected override Success Apply(Claim claim, SetSeverityInput input)
    {
        claim.SetSeverity(input.Severity, input.Now);

        return Success.Value;
    }
}
