using ClaimsModule.Domain.Claims.Issues.Inputs;
using ClaimsModule.Domain.Claims.Loss.Rules;
using ClaimsModule.Domain.Claims.Parties.Rules;
using ClaimsModule.Domain.Claims.RiskObjects.Rules;
using ClaimsModule.Domain.Claims.Rules;
using ClaimsModule.Domain.Common;
using ClaimsModule.Domain.Core.Flows;
using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.Claims.Issues.Flows;

internal sealed class EvaluateIssuesOperation
    : Operation<EvaluateIssuesOperation, Claim, EvaluateIssuesInput, IssueChanges>
{
    public EvaluateIssuesOperation()
    {
        Require(new IsClaimEditable());
    }

    protected override Outcome<IssueChanges> Apply(Claim claim, EvaluateIssuesInput input)
    {
        var findings = CheckIssueRules(claim, input);
        var findingKeys = findings
            .Select(finding => (finding.Code, finding.Evidence))
            .ToHashSet();
        var currentIssues = claim.Issues
            .Where(issue => issue.State.IsCurrent)
            .ToList();
        var currentKeys = currentIssues
            .Select(issue => (issue.Finding.Code, issue.Finding.Evidence))
            .ToHashSet();

        var resolved = currentIssues
            .Where(issue => !findingKeys.Contains((issue.Finding.Code, issue.Finding.Evidence)))
            .ToList();
        var added = findings
            .Where(finding => !currentKeys.Contains((finding.Code, finding.Evidence)))
            .ToList();

        foreach (var issue in resolved)
        {
            issue.Clear();
        }

        var raised = added
            .Select(finding => new ClaimValidationIssue(Ids.Next(), claim.Id, claim.OrganisationId, finding))
            .ToList();
        claim.IssueList.AddRange(raised);

        var changes = new IssueChanges(raised, resolved);

        return Done(changes);
    }

    private static IReadOnlyList<Finding> CheckIssueRules(Claim claim, EvaluateIssuesInput input)
    {
        var loss = claim.LossEvent;
        var reference = input.LossReference.RequireFor(claim.PolicyId);
        CheckResult[] results =
        [
            new HasValidLossDetails(loss.ReportDay, reference).Check(loss.Details),
            new HasClaimant().Check(claim),
            new HasLinkedPolicy().Check(claim),
            new HasRiskObject().Check(claim),
        ];

        return CheckResult.Combine(results).Findings;
    }
}
