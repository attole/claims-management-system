using ClaimsModule.Domain.Claims.Root;
using ClaimsModule.Domain.Engine.Operations;

namespace ClaimsModule.Domain.Claims.RiskObjects.Operations;

public sealed record AddRiskObjectInput(RiskObjectDetails Details, DateTimeOffset Now) : OperationInput(Now);

public sealed class AddRiskObjectOperation : Operation<Claim, AddRiskObjectInput, ClaimRiskObject>
{
    protected override ClaimRiskObject Apply(Claim claim, AddRiskObjectInput input)
    {
        return claim.AddRiskObject(input.Details, input.Now);
    }
}
