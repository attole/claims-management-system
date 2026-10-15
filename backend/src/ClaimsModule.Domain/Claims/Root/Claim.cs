using ClaimsModule.Domain.Claims.Issues;
using ClaimsModule.Domain.Claims.Issues.Events;
using ClaimsModule.Domain.Claims.Issues.Operations;
using ClaimsModule.Domain.Claims.Loss;
using ClaimsModule.Domain.Claims.Loss.Operations;
using ClaimsModule.Domain.Claims.Parties;
using ClaimsModule.Domain.Claims.Parties.Events;
using ClaimsModule.Domain.Claims.Parties.Operations;
using ClaimsModule.Domain.Claims.RiskObjects;
using ClaimsModule.Domain.Claims.RiskObjects.Operations;
using ClaimsModule.Domain.Claims.Root.Bases;
using ClaimsModule.Domain.Claims.Root.Events;
using ClaimsModule.Domain.Claims.Root.Operations;
using ClaimsModule.Domain.Claims.Root.Transitions;
using ClaimsModule.Domain.Claims.StateChanges;
using ClaimsModule.Domain.Common;
using ClaimsModule.Domain.Common.Capabilities;
using ClaimsModule.Domain.Common.Entities;
using ClaimsModule.Domain.Core.Base.Flows;
using ClaimsModule.Domain.Core.Operations;
using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.Claims.Root;

public sealed class Claim : AggregateRoot, IAuditable, ISoftDeletable, ITenantOwned, IStateful<ClaimState>
{
    private readonly List<ClaimParty> _parties = [];
    private readonly List<ClaimRiskObject> _riskObjects = [];
    private readonly List<ClaimValidationIssue> _issues = [];
    private readonly List<ClaimStateChange> _stateChanges = [];

    private Claim()
    {
        ClaimNumber = ClaimNumber.Empty;
        State = new ClaimDraftState();
        Severity = ClaimSeverity.Unspecified;
        LossEvent = new LossEvent(Guid.Empty, Guid.Empty, Guid.Empty, LossDetails.Empty, DateTimeOffset.MinValue);
    }

    internal Claim(Guid id, Guid organisationId, ClaimNumber claimNumber, DateTimeOffset now) : base(id)
    {
        OrganisationId = organisationId;
        ClaimNumber = claimNumber;
        State = new ClaimDraftState();
        Severity = ClaimSeverity.Unspecified;
        ReportedDate = now;
        CreatedAt = now;
        UpdatedAt = now;
        LossEvent = new LossEvent(Ids.Next(now), id, organisationId, LossDetails.Empty, now);
    }

    public ClaimNumber ClaimNumber { get; private set; }

    public ClaimState State { get; internal set; }

    public ClaimSeverity Severity { get; internal set; }

    public DateTimeOffset ReportedDate { get; private set; } = DateTimeOffset.MinValue;

    public Guid PolicyId { get; internal set; } = Guid.Empty;

    public Guid AssignedHandlerId { get; internal set; } = Guid.Empty;

    public string Notes { get; internal set; } = string.Empty;

    public bool HasAggregateLimitOverride { get; internal set; } = false;

    public LossEvent LossEvent { get; private set; }

    public IReadOnlyList<ClaimParty> Parties => _parties.AsReadOnly();

    public IReadOnlyList<ClaimRiskObject> RiskObjects => _riskObjects
        .Where(riskObject => !riskObject.IsDeleted)
        .ToList();

    public IReadOnlyList<ClaimValidationIssue> Issues => _issues.AsReadOnly();

    public IReadOnlyList<ClaimStateChange> StateChanges => _stateChanges.AsReadOnly();

    public IReadOnlyList<ClaimValidationIssue> BlockingIssues => _issues
        .Where(issue => issue.IsUnresolvedCritical)
        .ToList();

    public int ActiveClaimantCount => _parties.Count(party => party.IsActiveClaimant);

    internal List<ClaimParty> PartyList => _parties;

    internal List<ClaimRiskObject> RiskObjectList => _riskObjects;

    internal List<ClaimValidationIssue> IssueList => _issues;

    internal List<ClaimStateChange> StateChangeList => _stateChanges;

    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.MinValue;

    public DateTimeOffset UpdatedAt { get; private set; } = DateTimeOffset.MinValue;

    public Guid UserCreated { get; private set; } = Guid.Empty;

    public Guid UserModified { get; private set; } = Guid.Empty;

    public bool IsDeleted { get; private set; } = false;

    public DateTimeOffset DeletedAt { get; private set; } = DateTimeOffset.MinValue;

    public Guid OrganisationId { get; private set; } = Guid.Empty;

    public static FlowOutcome<Claim, DomainFinding> Create(CreateClaimInput input)
    {
        var outcome = new CreateClaimOperation().Execute(input);

        if (!outcome.IsSuccess)
        {
            return outcome;
        }

        var claim = outcome.Value;
        var created = new ClaimCreatedEvent(
            Ids.Next(input.Now),
            input.Now,
            input.ActorId,
            claim.Id,
            input.ClaimNumber.Value);
        claim.Raise(created);

        return claim.Settle(outcome, input);
    }

    public FlowOutcome<ClaimParty, DomainFinding> AddParty(AddPartyInput input)
    {
        var outcome = new AddPartyOperation().Execute(this, input);

        if (outcome.IsSuccess)
        {
            var party = outcome.Value;
            var added = new PartyAddedEvent(
                Ids.Next(input.Now),
                input.Now,
                input.ActorId,
                Id,
                party.Id,
                party.Details.Role.Name);
            Raise(added);
        }

        return Settle(outcome, input);
    }

    public FlowOutcome<ClaimParty, DomainFinding> RemoveParty(RemovePartyInput input)
    {
        var outcome = new RemovePartyOperation().Execute(this, input);

        if (outcome.IsSuccess)
        {
            var party = outcome.Value;
            var removed = new PartyRemovedEvent(
                Ids.Next(input.Now),
                input.Now,
                input.ActorId,
                Id,
                party.Id,
                party.Details.Role.Name);
            Raise(removed);
        }

        return Settle(outcome, input);
    }

    public FlowOutcome<ClaimRiskObject, DomainFinding> AddRiskObject(AddRiskObjectInput input)
    {
        var outcome = new AddRiskObjectOperation().Execute(this, input);

        return Settle(outcome, input);
    }

    public FlowOutcome<ClaimRiskObject, DomainFinding> RemoveRiskObject(RemoveRiskObjectInput input)
    {
        var outcome = new RemoveRiskObjectOperation().Execute(this, input);

        return Settle(outcome, input);
    }

    public FlowOutcome<LossEvent, DomainFinding> UpdateLossDetails(UpdateLossDetailsInput input)
    {
        var outcome = new UpdateLossDetailsOperation().Execute(this, input);

        return Settle(outcome, input);
    }

    public FlowOutcome<Success, DomainFinding> AssignHandler(AssignHandlerInput input)
    {
        var outcome = new AssignHandlerOperation().Execute(this, input);

        return Settle(outcome, input);
    }

    public FlowOutcome<Success, DomainFinding> SetSeverity(SetSeverityInput input)
    {
        var outcome = new SetSeverityOperation().Execute(this, input);

        return Settle(outcome, input);
    }

    public FlowOutcome<Success, DomainFinding> UpdateNotes(UpdateNotesInput input)
    {
        var outcome = new UpdateNotesOperation().Execute(this, input);

        return Settle(outcome, input);
    }

    public FlowOutcome<Success, DomainFinding> LinkPolicy(LinkPolicyInput input)
    {
        var outcome = new LinkPolicyOperation().Execute(this, input);

        return Settle(outcome, input);
    }

    public FlowOutcome<Success, DomainFinding> SetLimitOverride(SetLimitOverrideInput input)
    {
        var outcome = new SetLimitOverrideOperation().Execute(this, input);

        return Settle(outcome, input);
    }

    public FlowOutcome<ClaimValidationIssue, DomainFinding> WaiveIssue(WaiveIssueInput input)
    {
        var outcome = new WaiveIssueOperation().Execute(this, input);

        if (outcome.IsSuccess)
        {
            var issue = outcome.Value;
            var waived = new ValidationIssueWaivedEvent(
                Ids.Next(input.Now),
                input.Now,
                input.ActorId,
                Id,
                issue.Id,
                issue.Finding.Code);
            Raise(waived);
        }

        return Settle(outcome, input);
    }

    public FlowOutcome<ClaimValidationIssue, DomainFinding> AcknowledgeIssue(AcknowledgeIssueInput input)
    {
        var outcome = new AcknowledgeIssueOperation().Execute(this, input);

        if (outcome.IsSuccess)
        {
            var issue = outcome.Value;
            var acknowledged = new ValidationIssueAcknowledgedEvent(
                Ids.Next(input.Now),
                input.Now,
                input.ActorId,
                Id,
                issue.Id,
                issue.Finding.Code);
            Raise(acknowledged);
        }

        return Settle(outcome, input);
    }

    public FlowOutcome<IssueChanges, IssueFinding> EvaluateIssues(EvaluateIssuesInput input)
    {
        var outcome = new EvaluateIssuesFlow().Execute(this, input);
        RecordIssueChanges(outcome.Value, input);

        return outcome;
    }

    public FlowOutcome<ClaimState, DomainFinding> Open(OpenClaimInput input)
    {
        var outcome = new OpenClaimTransition().Execute(this, input);

        if (outcome.IsSuccess)
        {
            RecordMove(input);
        }

        return Settle(outcome, input);
    }

    public FlowOutcome<ClaimState, DomainFinding> Investigate(InvestigateClaimInput input)
    {
        var outcome = new InvestigateClaimTransition().Execute(this, input);

        if (outcome.IsSuccess)
        {
            RecordMove(input);
        }

        return Settle(outcome, input);
    }

    public FlowOutcome<ClaimState, DomainFinding> Withdraw(WithdrawClaimInput input)
    {
        var outcome = new WithdrawClaimTransition().Execute(this, input);

        if (outcome.IsSuccess)
        {
            RecordMove(input);
        }

        return Settle(outcome, input);
    }

    public FlowOutcome<ClaimState, DomainFinding> SendToPayment(SendToPaymentClaimInput input)
    {
        var outcome = new SendToPaymentClaimTransition().Execute(this, input);

        if (outcome.IsSuccess)
        {
            RecordMove(input);
        }

        return Settle(outcome, input);
    }

    public FlowOutcome<ClaimState, DomainFinding> Close(CloseClaimInput input)
    {
        var outcome = new CloseClaimTransition().Execute(this, input);

        if (outcome.IsSuccess)
        {
            RecordMove(input);
        }

        return Settle(outcome, input);
    }

    public FlowOutcome<ClaimState, DomainFinding> Reopen(ReopenClaimInput input)
    {
        var outcome = new ReopenClaimTransition().Execute(this, input);

        if (outcome.IsSuccess)
        {
            RecordMove(input);
            var open = new OpenClaimInput(input.ActorRole, input.ActorId, input.Now, input.Reference);
            var opened = new OpenClaimTransition().Execute(this, open);

            if (opened.IsSuccess)
            {
                RecordMove(open);
            }
        }

        return Settle(outcome, input);
    }

    private void Touch(DateTimeOffset now)
    {
        UpdatedAt = now;
    }

    private void RecordMove(ClaimBaseTransitionInput input)
    {
        var change = _stateChanges[^1];
        var moved = new ClaimStateChangedEvent(
            Ids.Next(input.Now),
            input.Now,
            input.ActorId,
            Id,
            change.From.Name,
            change.To.Name,
            input.Reason);
        Raise(moved);
    }

    private FlowOutcome<TValue, DomainFinding> Settle<TValue>(
        FlowOutcome<TValue, DomainFinding> outcome,
        ClaimBaseOperationInput input)
    {
        if (outcome.IsSuccess)
        {
            var evaluation = new EvaluateIssuesInput(input.Now, input.ActorId, input.Reference);
            Touch(input.Now);
            EvaluateIssues(evaluation);
        }

        return outcome;
    }

    private void RecordIssueChanges(IssueChanges changes, EvaluateIssuesInput input)
    {
        bool hasChanges = changes.Cleared.Count + changes.Raised.Count > 0;

        if (hasChanges)
        {
            Touch(input.Now);
        }

        foreach (var issue in changes.Cleared)
        {
            var cleared = new ValidationIssueClearedEvent(
                Ids.Next(input.Now),
                input.Now,
                input.ActorId,
                Id,
                issue.Id,
                issue.Finding.Code);
            Raise(cleared);
        }

        foreach (var issue in changes.Raised)
        {
            var raised = new ValidationIssueRaisedEvent(
                Ids.Next(input.Now),
                input.Now,
                input.ActorId,
                Id,
                issue.Id,
                issue.Finding.Code,
                issue.Finding.Severity.Name);
            Raise(raised);
        }
    }
}
