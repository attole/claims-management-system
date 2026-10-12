using ClaimsModule.Domain.Claims.Root;
using ClaimsModule.Domain.Common;
using ClaimsModule.Domain.Engine.Operations;

namespace ClaimsModule.Domain.Claims.RiskObjects.Operations;

public sealed record RemoveRiskObjectInput(Guid RiskObjectId, DateTimeOffset Now) : OperationInput(Now);

public sealed class RemoveRiskObjectOperation : Operation<Claim, RemoveRiskObjectInput, Success>
{
    protected override Success Apply(Claim claim, RemoveRiskObjectInput input)
    {
        claim.RemoveRiskObject(input.RiskObjectId, input.Now);

        return Success.Value;
    }
}
