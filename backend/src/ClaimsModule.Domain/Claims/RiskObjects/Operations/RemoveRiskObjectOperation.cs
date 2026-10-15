using ClaimsModule.Domain.Claims.RiskObjects.Rules;
using ClaimsModule.Domain.Claims.Root;
using ClaimsModule.Domain.Claims.Root.Bases;
using ClaimsModule.Domain.Claims.Root.Rules;
using ClaimsModule.Domain.Core.Operations;

namespace ClaimsModule.Domain.Claims.RiskObjects.Operations;

public sealed record RemoveRiskObjectInput(Guid RiskObjectId, Guid ActorId, DateTimeOffset Now)
    : ClaimBaseOperationInput(Now, ActorId);

public sealed class RemoveRiskObjectOperation : Operation<Claim, RemoveRiskObjectInput, Success>
{
    public RemoveRiskObjectOperation()
    {
        Require(new IsClaimEditable());
        Require(input => new IsRiskObjectPresent(input.RiskObjectId));
    }

    protected override Success Apply(Claim claim, RemoveRiskObjectInput input)
    {
        claim.RemoveRiskObject(input.RiskObjectId, input.Now);

        return Success.Value;
    }
}
