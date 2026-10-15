using ClaimsModule.Domain.Claims.Root;
using ClaimsModule.Domain.Core.Base.Checks;
using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.Claims.RiskObjects.Rules;

internal sealed class HasNoOtherPrimaryRiskObject(Guid riskObjectId, bool isPrimary) : IRule<Claim>
{
    public const string Code = "RISK_OBJECT_PRIMARY_EXISTS";

    private const string Message = "Another risk object is already the primary one.";
    private const string Field = "IsPrimary";

    public CheckResult<DomainFinding> Check(Claim claim)
    {
        bool hasOtherPrimary = claim.RiskObjects
            .Where(riskObject => riskObject.Id != riskObjectId)
            .Any(riskObject => riskObject.Details.IsPrimary);
        bool hasNoOtherPrimary = !isPrimary || !hasOtherPrimary;

        return hasNoOtherPrimary
            ? CheckResult<DomainFinding>.Pass
            : CheckResult<DomainFinding>.Found(new RuleFinding(Code, Message, Field));
    }
}
