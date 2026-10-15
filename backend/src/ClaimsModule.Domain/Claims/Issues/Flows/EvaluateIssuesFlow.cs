using ClaimsModule.Domain.Claims.Issues.Checks;
using ClaimsModule.Domain.Claims.Issues.Inputs;
using ClaimsModule.Domain.Claims.Root;
using ClaimsModule.Domain.Common;
using ClaimsModule.Domain.Core.Base.Checks;
using ClaimsModule.Domain.Core.Base.Flows;
using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.Claims.Issues.Flows;

internal sealed class EvaluateIssuesFlow
    : Flow<EvaluateIssuesFlow, Claim, EvaluateIssuesInput, IssueChanges, DomainFinding>
{
    public EvaluateIssuesFlow()
    {
        Require(input => new HasValidLoss(input.Now, input.Reference));
        Require(new HasClaimant());
        Require(new HasLinkedPolicy());
        Require(new HasRiskObject());
    }

    protected override FlowDecision WhenBlocked => FlowDecision.Continue;

    protected override IssueChanges Apply(Claim claim, EvaluateIssuesInput input, CheckResult<DomainFinding> result)
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
            issue.Clear(input.ActorId, input.Now);
        }

        var raised = added
            .Select(finding => new ClaimValidationIssue(
                Ids.Next(input.Now),
                claim.Id,
                claim.OrganisationId,
                IssueFinding.From(finding),
                input.Now))
            .ToList();
        claim.IssueList.AddRange(raised);

        return new IssueChanges(raised, resolved);
    }
}
