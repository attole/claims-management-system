using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.Claims.Issues.Rules;

internal sealed class IsIssueAcknowledgeable : IRule<ClaimValidationIssue>
{
    public const string Code = "ISSUE_NOT_ACKNOWLEDGEABLE";

    private const string Message = "Only an unresolved warning can be acknowledged.";
    private const string Field = "IssueId";

    public CheckResult Check(ClaimValidationIssue issue)
    {
        bool isAcknowledgeable = issue.IsUnresolvedWarning;

        return isAcknowledgeable
            ? CheckResult.Pass
            : CheckResult.Found(new Finding(Code, Message, Field));
    }
}
