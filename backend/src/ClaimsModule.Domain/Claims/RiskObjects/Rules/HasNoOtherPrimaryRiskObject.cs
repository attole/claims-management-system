using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.Claims.RiskObjects.Rules;

internal sealed class HasNoOtherPrimaryRiskObject(Guid riskObjectId, bool isPrimary) : IRule<Claim>
{
    public const string Code = "RISK_OBJECT_PRIMARY_EXISTS";

    private const string Message = "Another risk object is already the primary one.";
    private const string Field = "IsPrimary";

    public CheckResult Check(Claim claim)
    {
        bool hasNoOtherPrimary = !isPrimary || !claim.HasOtherPrimaryRiskObject(riskObjectId);

        return hasNoOtherPrimary
            ? CheckResult.Pass
            : CheckResult.Found(new Finding(Code, Message, Field));
    }
}
