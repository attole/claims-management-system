using ClaimsModule.Domain.Claims.Issues.Checks;
using ClaimsModule.Domain.Claims.Root;
using ClaimsModule.Domain.Claims.Root.Bases;
using ClaimsModule.Domain.Core.Base.Checks;
using ClaimsModule.Domain.Core.Base.Flows;
using ClaimsModule.Domain.Core.Operations;

namespace ClaimsModule.Domain.Claims.Issues;

public sealed record EvaluateIssuesInput(DateTimeOffset Now, Guid ActorId, ClaimReferenceSnapshot Reference)
    : ClaimBaseOperationInput(Now, ActorId);

public sealed class EvaluateIssuesFlow : Flow<Claim, EvaluateIssuesInput, Success, IssueFinding>
{
    public EvaluateIssuesFlow()
    {
        Require(new IsLossDatePresent());
        Require(input => new IsLossDateNotInFuture(input.Now));
        Require(new IsDescriptionLongEnough());
        Require(new HasClaimant());
        Require(input => new IsCauseCodeActive(input.Reference.ActiveCauseCodes));
        Require(new HasLinkedPolicy());
        Require(input => new IsLossDateWithinPolicyPeriod(input.Reference.PolicyPeriod));
    }

    protected override FlowDecision WhenBlocked => FlowDecision.Continue;

    protected override Success Apply(Claim claim, EvaluateIssuesInput input, CheckResult<IssueFinding> result)
    {
        var findingCodes = result.Findings
            .Select(finding => finding.Code)
            .ToHashSet();
        var currentIssues = claim.Issues
            .Where(issue => issue.IsCurrent)
            .ToList();
        var currentCodes = currentIssues
            .Select(issue => issue.Finding.Code)
            .ToHashSet();

        var resolved = currentIssues
            .Where(issue => !findingCodes.Contains(issue.Finding.Code))
            .ToList();
        var added = result.Findings
            .Where(finding => !currentCodes.Contains(finding.Code))
            .ToList();

        foreach (var issue in resolved)
        {
            claim.ClearIssue(issue, input.ActorId, input.Now);
        }

        foreach (var finding in added)
        {
            claim.RaiseIssue(finding, input.ActorId, input.Now);
        }

        return Success.Value;
    }
}
