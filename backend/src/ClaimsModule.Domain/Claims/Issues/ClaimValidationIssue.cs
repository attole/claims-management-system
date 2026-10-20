using ClaimsModule.Domain.Claims.Issues.Events;
using ClaimsModule.Domain.Claims.Issues.Flows;
using ClaimsModule.Domain.Claims.Issues.Inputs;
using ClaimsModule.Domain.Claims.Loss.Rules;
using ClaimsModule.Domain.Claims.Parties.Rules;
using ClaimsModule.Domain.Common.Capabilities;
using ClaimsModule.Domain.Common.Entities;
using ClaimsModule.Domain.Core.Flows;
using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.Claims.Issues;

public sealed class ClaimValidationIssue : Entity, ISoftDeletable
{
    private static readonly string[] _fixOnlyCodes = [IsLossDatePresent.Code, IsCauseCodeActive.Code, HasClaimant.Code];

    private ClaimValidationIssue()
    {
        Finding = new Finding(string.Empty, string.Empty);
        State = new IssueRaisedState();
    }

    internal ClaimValidationIssue(Guid id, Guid claimId, Guid organisationId, Finding finding) : base(id)
    {
        ClaimId = claimId;
        OrganisationId = organisationId;
        Finding = finding;
        State = new IssueRaisedState();
    }

    public Guid ClaimId { get; private set; } = Guid.Empty;

    public Finding Finding { get; private set; }

    public IssueState State { get; internal set; }

    public DateTimeOffset ResolvedAt { get; internal set; } = DateTimeOffset.MinValue;

    public Guid ResolvedBy { get; internal set; } = Guid.Empty;

    public bool IsUnresolvedCritical => !State.IsResolved && Finding.Severity == FindingSeverity.Critical;

    public bool IsUnresolvedWarning => !State.IsResolved && Finding.Severity == FindingSeverity.Warning;

    public bool HasWaivableFinding => !_fixOnlyCodes.Any(code => Finding.HasCode(code));

    public bool IsDeleted { get; private set; } = false;

    internal Outcome<ClaimValidationIssue> Clear()
    {
        return ClearIssueOperation.Run(this, NoInput.Value)
            .OnSuccess(issue => Raise(new ValidationIssueClearedEvent(ClaimId, issue.Id, issue.Finding.Code)));
    }

    internal Outcome<ClaimValidationIssue> Acknowledge(IssueResolutionInput input)
    {
        return AcknowledgeIssueOperation.Run(this, input)
            .OnSuccess(issue => Raise(new ValidationIssueAcknowledgedEvent(ClaimId, issue.Id, issue.Finding.Code)));
    }

    internal Outcome<ClaimValidationIssue> Waive(IssueResolutionInput input)
    {
        return WaiveIssueOperation.Run(this, input)
            .OnSuccess(issue => Raise(new ValidationIssueWaivedEvent(ClaimId, issue.Id, issue.Finding.Code)));
    }
}
