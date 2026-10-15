using ClaimsModule.Domain.Claims.Issues;
using ClaimsModule.Domain.Claims.Root.Bases;
using ClaimsModule.Domain.Claims.Root.Rules;
using ClaimsModule.Domain.Core.Operations;

namespace ClaimsModule.Domain.Claims.Root.Operations;

public sealed record SetSeverityInput(
    ClaimSeverity Severity,
    Guid ActorId,
    DateTimeOffset Now,
    ClaimReferenceSnapshot Reference) : ClaimBaseOperationInput(Now, ActorId, Reference);

internal sealed class SetSeverityOperation : Operation<Claim, SetSeverityInput, Success>
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
