using ClaimsModule.Domain.Claims.Root;
using ClaimsModule.Domain.Core.Base.Checks;
using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.Claims.Issues.Rules;

internal sealed class IsIssueAcknowledgeable(Guid issueId) : IRule<Claim>
{
    public const string Code = "ISSUE_NOT_ACKNOWLEDGEABLE";

    private const string Message = "Only an unresolved warning can be acknowledged.";
    private const string Field = "IssueId";

    public CheckResult<DomainFinding> Check(Claim claim)
    {
        bool isAcknowledgeable = claim.Issues.Any(issue => issue.Id == issueId && issue.IsUnresolvedWarning);

        return isAcknowledgeable
            ? CheckResult<DomainFinding>.Pass
            : CheckResult<DomainFinding>.Found(new RuleFinding(Code, Message, Field));
    }
}
