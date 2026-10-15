using ClaimsModule.Domain.Claims.Loss.Checks;
using ClaimsModule.Domain.Core.Base.Checks;
using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.Claims.Root.Rules;

internal sealed class HasNoUnacknowledgedPolicyWarning : IRule<Claim>
{
    public const string Code = "S:BR-C-02";

    private const string Message = "The policy period warning must be cleared or acknowledged before opening.";
    private const string Field = "Issues";

    public CheckResult<DomainFinding> Check(Claim claim)
    {
        bool hasNoUnacknowledgedWarning = !claim.Issues.Any(
            issue => issue.Finding.Code == IsLossDateWithinPolicyPeriod.Code && issue.IsUnresolvedWarning);

        return hasNoUnacknowledgedWarning
            ? CheckResult<DomainFinding>.Pass
            : CheckResult<DomainFinding>.Found(new RuleFinding(Code, Message, Field));
    }
}
