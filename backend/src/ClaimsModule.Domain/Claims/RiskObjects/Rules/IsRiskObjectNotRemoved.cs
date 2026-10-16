using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.Claims.RiskObjects.Rules;

internal sealed class IsRiskObjectNotRemoved : IRule<ClaimRiskObject>
{
    public const string Code = "RISK_OBJECT_REMOVED";

    private const string Message = "The risk object has been removed.";
    private const string Field = "RiskObjectId";

    public CheckResult Check(ClaimRiskObject riskObject)
    {
        bool isNotRemoved = !riskObject.IsDeleted;

        return isNotRemoved
            ? CheckResult.Pass
            : CheckResult.Found(new Finding(Code, Message, Field));
    }
}
