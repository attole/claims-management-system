using ClaimsModule.Domain.Claims.Issues;
using ClaimsModule.Domain.Claims.Issues.Events;
using ClaimsModule.Domain.Claims.Issues.Flows;
using ClaimsModule.Domain.Claims.Issues.Inputs;
using ClaimsModule.Domain.Claims.Loss;
using ClaimsModule.Domain.Claims.Loss.Flows;
using ClaimsModule.Domain.Claims.Loss.Inputs;
using ClaimsModule.Domain.Claims.Parties;
using ClaimsModule.Domain.Claims.Parties.Events;
using ClaimsModule.Domain.Claims.Parties.Flows;
using ClaimsModule.Domain.Claims.Parties.Inputs;
using ClaimsModule.Domain.Claims.RiskObjects;
using ClaimsModule.Domain.Claims.RiskObjects.Flows;
using ClaimsModule.Domain.Claims.RiskObjects.Inputs;
using ClaimsModule.Domain.Claims.Root.Events;
using ClaimsModule.Domain.Claims.Root.Flows;
using ClaimsModule.Domain.Claims.Root.Inputs;
using ClaimsModule.Domain.Claims.StateChanges;
using ClaimsModule.Domain.Common;
using ClaimsModule.Domain.Common.Capabilities;
using ClaimsModule.Domain.Common.Entities;
using ClaimsModule.Domain.Core.Base.Flows;
using ClaimsModule.Domain.Core.Operations;
using ClaimsModule.Domain.Core.Rules;
using ClaimsModule.Domain.Core.Transitions;

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
        return CreateClaimOperation.Execute(input)
            .OnSuccess(claim => claim.Raise(new ClaimCreatedEvent(input.Stamp(), claim.Id, input.ClaimNumber.Value)))
            .OnSuccess(claim => claim.Settle(input));
    }

    public FlowOutcome<ClaimParty, DomainFinding> AddParty(AddPartyInput input)
    {
        return AddPartyOperation.Execute(this, input)
            .OnSuccess(party => Raise(new PartyAddedEvent(input.Stamp(), Id, party.Id, party.Details.Role.Name)))
            .OnSuccess(() => Settle(input));
    }

    public FlowOutcome<ClaimParty, DomainFinding> RemoveParty(RemovePartyInput input)
    {
        return RemovePartyOperation.Execute(this, input)
            .OnSuccess(party => Raise(new PartyRemovedEvent(input.Stamp(), Id, party.Id, party.Details.Role.Name)))
            .OnSuccess(() => Settle(input));
    }

    public FlowOutcome<ClaimRiskObject, DomainFinding> AddRiskObject(AddRiskObjectInput input)
    {
        return AddRiskObjectOperation.Execute(this, input).OnSuccess(() => Settle(input));
    }

    public FlowOutcome<ClaimRiskObject, DomainFinding> RemoveRiskObject(RemoveRiskObjectInput input)
    {
        return RemoveRiskObjectOperation.Execute(this, input).OnSuccess(() => Settle(input));
    }

    public FlowOutcome<LossEvent, DomainFinding> UpdateLossDetails(UpdateLossDetailsInput input)
    {
        return UpdateLossDetailsOperation.Execute(this, input).OnSuccess(() => Settle(input));
    }

    public FlowOutcome<Success, DomainFinding> AssignHandler(AssignHandlerInput input)
    {
        return AssignHandlerOperation.Execute(this, input).OnSuccess(() => Settle(input));
    }

    public FlowOutcome<Success, DomainFinding> SetSeverity(SetSeverityInput input)
    {
        return SetSeverityOperation.Execute(this, input).OnSuccess(() => Settle(input));
    }

    public FlowOutcome<Success, DomainFinding> UpdateNotes(UpdateNotesInput input)
    {
        return UpdateNotesOperation.Execute(this, input).OnSuccess(() => Settle(input));
    }

    public FlowOutcome<Success, DomainFinding> LinkPolicy(LinkPolicyInput input)
    {
        return LinkPolicyOperation.Execute(this, input).OnSuccess(() => Settle(input));
    }

    public FlowOutcome<Success, DomainFinding> SetLimitOverride(SetLimitOverrideInput input)
    {
        return SetLimitOverrideOperation.Execute(this, input).OnSuccess(() => Settle(input));
    }

    public FlowOutcome<ClaimValidationIssue, DomainFinding> WaiveIssue(WaiveIssueInput input)
    {
        return WaiveIssueOperation.Execute(this, input)
            .OnSuccess(issue => Raise(new ValidationIssueWaivedEvent(input.Stamp(), Id, issue.Id, issue.Finding.Code)))
            .OnSuccess(() => Settle(input));
    }

    public FlowOutcome<ClaimValidationIssue, DomainFinding> AcknowledgeIssue(AcknowledgeIssueInput input)
    {
        return AcknowledgeIssueOperation.Execute(this, input)
            .OnSuccess(issue => Raise(new ValidationIssueAcknowledgedEvent(
                input.Stamp(),
                Id,
                issue.Id,
                issue.Finding.Code)))
            .OnSuccess(() => Settle(input));
    }

    public FlowOutcome<IssueChanges, IssueFinding> EvaluateIssues(EvaluateIssuesInput input)
    {
        return EvaluateIssuesFlow.Execute(this, input).OnSuccess(changes => RecordIssueChanges(changes, input));
    }

    public FlowOutcome<StateChange<ClaimState>, DomainFinding> Open(OpenClaimInput input)
    {
        return OpenClaimTransition.Execute(this, input)
            .OnSuccess(change => RecordMove(change, input))
            .OnSuccess(() => Settle(input));
    }

    public FlowOutcome<StateChange<ClaimState>, DomainFinding> Investigate(InvestigateClaimInput input)
    {
        return InvestigateClaimTransition.Execute(this, input)
            .OnSuccess(change => RecordMove(change, input))
            .OnSuccess(() => Settle(input));
    }

    public FlowOutcome<StateChange<ClaimState>, DomainFinding> Withdraw(WithdrawClaimInput input)
    {
        return WithdrawClaimTransition.Execute(this, input)
            .OnSuccess(change => RecordMove(change, input))
            .OnSuccess(() => Settle(input));
    }

    public FlowOutcome<StateChange<ClaimState>, DomainFinding> SendToPayment(SendToPaymentClaimInput input)
    {
        return SendToPaymentClaimTransition.Execute(this, input)
            .OnSuccess(change => RecordMove(change, input))
            .OnSuccess(() => Settle(input));
    }

    public FlowOutcome<StateChange<ClaimState>, DomainFinding> Close(CloseClaimInput input)
    {
        return CloseClaimTransition.Execute(this, input)
            .OnSuccess(change => RecordMove(change, input))
            .OnSuccess(() => Settle(input));
    }

    public FlowOutcome<StateChange<ClaimState>, DomainFinding> Reopen(ReopenClaimInput input)
    {
        return ReopenClaimTransition.Execute(this, input)
            .OnSuccess(change => RecordMove(change, input))
            .OnSuccess(() => OpenAfterReopen(input))
            .OnSuccess(() => Settle(input));
    }

    private void Touch(DateTimeOffset now)
    {
        UpdatedAt = now;
    }

    private void Settle(ClaimBaseOperationInput input)
    {
        var evaluation = new EvaluateIssuesInput(input.Now, input.ActorId, input.Reference);
        Touch(input.Now);
        EvaluateIssues(evaluation);
    }

    private void RecordMove(StateChange<ClaimState> change, ClaimBaseTransitionInput input)
    {
        Raise(new ClaimStateChangedEvent(input.Stamp(), Id, change.From.Name, change.To.Name, input.Reason));
    }

    private void OpenAfterReopen(ReopenClaimInput input)
    {
        var open = new OpenClaimInput(input.ActorRole, input.ActorId, input.Now, input.Reference);
        OpenClaimTransition.Execute(this, open).OnSuccess(change => RecordMove(change, open));
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
            Raise(new ValidationIssueClearedEvent(input.Stamp(), Id, issue.Id, issue.Finding.Code));
        }

        foreach (var issue in changes.Raised)
        {
            Raise(new ValidationIssueRaisedEvent(
                input.Stamp(),
                Id,
                issue.Id,
                issue.Finding.Code,
                issue.Finding.Severity.Name));
        }
    }

}
