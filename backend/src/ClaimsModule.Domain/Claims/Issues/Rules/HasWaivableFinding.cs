using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.Claims.Issues.Rules;

internal sealed class HasWaivableFinding : IRule<ClaimValidationIssue>
{
    public const string Code = "ISSUE_MUST_BE_FIXED";

    private const string Message = "This issue cannot be waived; correct the claim data instead.";
    private const string Field = "IssueId";

    public CheckResult Check(ClaimValidationIssue issue)
    {
        bool hasWaivableFinding = issue.HasWaivableFinding;

        return hasWaivableFinding
            ? CheckResult.Pass
            : CheckResult.Found(new Finding(Code, Message, Field));
    }
}
