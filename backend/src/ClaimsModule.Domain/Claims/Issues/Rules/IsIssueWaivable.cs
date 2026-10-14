using ClaimsModule.Domain.Claims.Root;
using ClaimsModule.Domain.Core.Base.Checks;
using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.Claims.Issues.Rules;

public sealed class IsIssueWaivable(Guid issueId) : IRule<Claim>
{
    public const string Code = "ISSUE_NOT_WAIVABLE";

    private const string Message = "Only an unresolved critical issue can be waived.";
    private const string Field = "IssueId";

    public CheckResult<DomainFinding> Check(Claim claim)
    {
        bool isWaivable = claim.Issues.Any(issue => issue.Id == issueId && issue.IsUnresolvedCritical);

        return isWaivable
            ? CheckResult<DomainFinding>.Pass
            : CheckResult<DomainFinding>.Found(new RuleFinding(Code, Message, Field));
    }
}
