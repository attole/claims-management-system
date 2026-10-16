using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.Claims.Issues.Rules;

internal sealed class IsIssueWaivable : IRule<ClaimValidationIssue>
{
    public const string Code = "ISSUE_NOT_WAIVABLE";

    private const string Message = "Only an unresolved critical issue can be waived.";
    private const string Field = "IssueId";

    public CheckResult Check(ClaimValidationIssue issue)
    {
        bool isWaivable = issue.IsUnresolvedCritical;

        return isWaivable
            ? CheckResult.Pass
            : CheckResult.Found(new Finding(Code, Message, Field));
    }
}
