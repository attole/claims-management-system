using ClaimsModule.Domain.Core.Base.Checks;
using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.Claims.Root.Rules;

internal sealed class HasNoUnresolvedCriticalIssues : IRule<Claim>
{
    public const string Code = "S:CC-02";

    private const string Message = "Claim cannot be closed - clearing of every Critical issue is not satisfied.";
    private const string Field = "Issues";

    public CheckResult<DomainFinding> Check(Claim claim)
    {
        bool hasNoUnresolvedCriticalIssues = claim.BlockingIssues.Count == 0;

        return hasNoUnresolvedCriticalIssues
            ? CheckResult<DomainFinding>.Pass
            : CheckResult<DomainFinding>.Found(new RuleFinding(Code, Message, Field));
    }
}
