using ClaimsModule.Domain.Claims.Root;
using ClaimsModule.Domain.Core.Base.Checks;
using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.Claims.Issues.Checks;

internal sealed class HasRiskObject : ICheck<Claim, DomainFinding>
{
    public const string Code = "CLAIM_NO_RISK_OBJECT";

    private const string Message = "No risk object is linked to the claim.";
    private const string Field = "RiskObjects";

    public CheckResult<DomainFinding> Check(Claim claim)
    {
        bool hasRiskObject = claim.RiskObjects.Count > 0;

        return hasRiskObject
            ? CheckResult<DomainFinding>.Pass
            : CheckResult<DomainFinding>.Found(new RuleFinding(Code, Message, Field).AsWarning());
    }
}
