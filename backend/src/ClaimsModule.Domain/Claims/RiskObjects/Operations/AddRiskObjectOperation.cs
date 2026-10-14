using ClaimsModule.Domain.Claims.Root;

namespace ClaimsModule.Domain.Claims.RiskObjects.Operations;

public sealed record AddRiskObjectInput(RiskObjectDetails Details, Guid ActorId, DateTimeOffset Now)
    : ClaimBaseInput(Now, ActorId);

public sealed class AddRiskObjectOperation : ClaimBaseOperation<AddRiskObjectInput, ClaimRiskObject>
{
    protected override ClaimRiskObject Apply(Claim claim, AddRiskObjectInput input)
    {
        return claim.AddRiskObject(input.Details, input.Now);
    }
}
