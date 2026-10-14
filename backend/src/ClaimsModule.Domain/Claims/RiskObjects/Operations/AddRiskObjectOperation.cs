using ClaimsModule.Domain.Claims.Root;
using ClaimsModule.Domain.Claims.Root.Bases;
using ClaimsModule.Domain.Claims.Root.Rules;

namespace ClaimsModule.Domain.Claims.RiskObjects.Operations;

public sealed record AddRiskObjectInput(RiskObjectDetails Details, Guid ActorId, DateTimeOffset Now)
    : ClaimBaseOperationInput(Now, ActorId);

public sealed class AddRiskObjectOperation : ClaimBaseOperation<AddRiskObjectInput, ClaimRiskObject>
{
    public AddRiskObjectOperation()
    {
        Require(new IsClaimEditable());
    }

    protected override ClaimRiskObject Apply(Claim claim, AddRiskObjectInput input)
    {
        return claim.AddRiskObject(input.Details, input.Now);
    }
}
