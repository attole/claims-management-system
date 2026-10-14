using ClaimsModule.Domain.Claims.Root;
using ClaimsModule.Domain.Core.Base.Checks;
using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.Claims.RiskObjects.Rules;

public sealed class IsRiskObjectPresent(Guid riskObjectId) : IRule<Claim>
{
    public const string Code = "RISK_OBJECT_NOT_PRESENT";

    private const string Message = "The risk object is not on the claim or has been removed.";
    private const string Field = "RiskObjectId";

    public CheckResult<DomainFinding> Check(Claim claim)
    {
        bool isPresent = claim.RiskObjects.Any(riskObject => riskObject.Id == riskObjectId);

        return isPresent
            ? CheckResult<DomainFinding>.Pass
            : CheckResult<DomainFinding>.Found(new RuleFinding(Code, Message, Field));
    }
}
