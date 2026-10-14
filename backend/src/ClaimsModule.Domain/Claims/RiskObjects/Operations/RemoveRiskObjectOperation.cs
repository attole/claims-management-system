using ClaimsModule.Domain.Claims.RiskObjects.Rules;
using ClaimsModule.Domain.Claims.Root;
using ClaimsModule.Domain.Core.Operations;

namespace ClaimsModule.Domain.Claims.RiskObjects.Operations;

public sealed record RemoveRiskObjectInput(Guid RiskObjectId, Guid ActorId, DateTimeOffset Now)
    : ClaimBaseInput(Now, ActorId);

public sealed class RemoveRiskObjectOperation : ClaimBaseOperation<RemoveRiskObjectInput, Success>
{
    public RemoveRiskObjectOperation()
    {
        Require(input => new IsRiskObjectPresent(input.RiskObjectId));
    }

    protected override Success Apply(Claim claim, RemoveRiskObjectInput input)
    {
        claim.RemoveRiskObject(input.RiskObjectId, input.Now);

        return Success.Value;
    }
}
