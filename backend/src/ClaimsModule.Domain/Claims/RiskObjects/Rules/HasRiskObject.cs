using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.Claims.RiskObjects.Rules;

internal sealed class HasRiskObject : IRule<Claim>
{
    public const string Code = "CLAIM_NO_RISK_OBJECT";

    private const string Message = "No risk object is linked to the claim.";
    private const string Field = "RiskObjects";

    public CheckResult Check(Claim claim)
    {
        bool hasRiskObject = claim.HasActiveRiskObject;

        return hasRiskObject
            ? CheckResult.Pass
            : CheckResult.Found(new Finding(Code, Message, Field).AsWarning());
    }
}
