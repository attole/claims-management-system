using ClaimsModule.Domain.Claims.Events;
using ClaimsModule.Domain.Claims.Flows;
using ClaimsModule.Domain.Claims.Inputs;
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
using ClaimsModule.Domain.Claims.StateChanges;
using ClaimsModule.Domain.Common;
using ClaimsModule.Domain.Common.Capabilities;
using ClaimsModule.Domain.Common.Entities;
using ClaimsModule.Domain.Core.Flows;
using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.Claims;

public sealed class Claim : AggregateRoot, ISoftDeletable, IStateful<ClaimState>
{
    private const decimal ReserveLimit = 10_000_000m;

    private readonly List<ClaimParty> _parties = [];
    private readonly List<ClaimRiskObject> _riskObjects = [];
    private readonly List<ClaimValidationIssue> _issues = [];
    private readonly List<ClaimStateChange> _stateChanges = [];

    private Claim()
    {
        ClaimNumber = ClaimNumber.Empty;
        State = new ClaimDraftState();
        Severity = ClaimSeverity.Unspecified;
        LossEvent = new ClaimLossEvent(Guid.Empty, Guid.Empty, Guid.Empty, LossDetails.Empty, DateTimeOffset.MinValue);
    }

    internal Claim(Guid id, Guid organisationId, ClaimNumber claimNumber, DateTimeOffset reportDate, DateTimeOffset now)
        : base(id)
    {
        OrganisationId = organisationId;
        ClaimNumber = claimNumber;
        State = new ClaimDraftState();
        Severity = ClaimSeverity.Unspecified;
        ReportedDate = now;
        LossEvent = new ClaimLossEvent(Ids.Next(), id, organisationId, LossDetails.Empty, reportDate);
    }

    public ClaimNumber ClaimNumber { get; private set; }

    public ClaimState State { get; internal set; }

    public ClaimSeverity Severity { get; internal set; }

    public DateTimeOffset ReportedDate { get; private set; } = DateTimeOffset.MinValue;

    public Guid PolicyId { get; internal set; } = Guid.Empty;

    public Guid AssignedHandlerId { get; internal set; } = Guid.Empty;

    public string Notes { get; internal set; } = string.Empty;

    public bool HasAggregateLimitOverride { get; internal set; } = false;

    public ClaimLossEvent LossEvent { get; private set; }

    public IReadOnlyList<ClaimParty> Parties => _parties.AsReadOnly();

    public IReadOnlyList<ClaimRiskObject> RiskObjects => _riskObjects.AsReadOnly();

    public IReadOnlyList<ClaimValidationIssue> Issues => _issues.AsReadOnly();

    public IReadOnlyList<ClaimStateChange> StateChanges => _stateChanges.AsReadOnly();

    internal List<ClaimParty> PartyList => _parties;

    internal List<ClaimRiskObject> RiskObjectList => _riskObjects;

    internal List<ClaimValidationIssue> IssueList => _issues;

    internal List<ClaimStateChange> StateChangeList => _stateChanges;

    public bool HasActiveClaimant => _parties.Any(party => party.IsActiveClaimant);

    public bool HasAssignedHandler => AssignedHandlerId != Guid.Empty;

    public bool HasLinkedPolicy => PolicyId != Guid.Empty;

    public bool HasActiveRiskObject => _riskObjects.Any(riskObject => !riskObject.IsDeleted);

    public bool HasUnresolvedCriticalIssues => _issues.Any(issue => issue.IsUnresolvedCritical);

    public bool IsDeleted { get; private set; } = false;

    public DateTimeOffset DeletedAt { get; private set; } = DateTimeOffset.MinValue;

    public static Outcome<Claim> Create(CreateClaimInput input)
    {
        return CreateClaimOperation.Run(input)
            .OnSuccess(claim => claim.Raise(new ClaimCreatedEvent(claim.Id, input.ClaimNumber.Value)))
            .OnSuccess(claim => claim.Settle(input));
    }

    public Outcome<ClaimParty> AddParty(AddPartyInput input)
    {
        return AddPartyOperation.Run(this, input)
            .OnSuccess(party => Raise(new PartyAddedEvent(Id, party.Id, party.Details.Role.Name)))
            .OnSuccess(() => Settle(input));
    }

    public Outcome<ClaimParty> RemoveParty(RemovePartyInput input)
    {
        return RemovePartyOnClaimOperation.Run(this, input).OnSuccess(() => Settle(input));
    }

    public Outcome<ClaimRiskObject> AddRiskObject(AddRiskObjectInput input)
    {
        return AddRiskObjectOperation.Run(this, input).OnSuccess(() => Settle(input));
    }

    public Outcome<ClaimRiskObject> UpdateRiskObject(UpdateRiskObjectInput input)
    {
        return UpdateRiskObjectOnClaimOperation.Run(this, input).OnSuccess(() => Settle(input));
    }

    public Outcome<ClaimRiskObject> RemoveRiskObject(RemoveRiskObjectInput input)
    {
        return RemoveRiskObjectOnClaimOperation.Run(this, input).OnSuccess(() => Settle(input));
    }

    public Outcome<ClaimLossEvent> UpdateLossDetails(UpdateLossDetailsInput input)
    {
        return UpdateLossDetailsOnClaimOperation.Run(this, input).OnSuccess(() => Settle(input));
    }

    public Outcome<Claim> AssignHandler(AssignHandlerInput input)
    {
        return AssignHandlerOperation.Run(this, input).OnSuccess(() => Settle(input));
    }

    public Outcome<Claim> SetSeverity(SetSeverityInput input)
    {
        return SetSeverityOperation.Run(this, input).OnSuccess(() => Settle(input));
    }

    public Outcome<Claim> UpdateNotes(UpdateNotesInput input)
    {
        return UpdateNotesOperation.Run(this, input).OnSuccess(() => Settle(input));
    }

    public Outcome<Claim> LinkPolicy(LinkPolicyInput input)
    {
        return LinkPolicyOperation.Run(this, input).OnSuccess(() => Settle(input));
    }

    public Outcome<Claim> SetLimitOverride(SetLimitOverrideInput input)
    {
        return SetLimitOverrideOperation.Run(this, input)
            .OnSuccess(claim => Raise(new AggregateLimitOverrideChangedEvent(Id, claim.HasAggregateLimitOverride)))
            .OnSuccess(() => Settle(input));
    }

    public Outcome<ClaimValidationIssue> WaiveIssue(WaiveIssueInput input)
    {
        return WaiveIssueOnClaimOperation.Run(this, input).OnSuccess(() => Settle(input));
    }

    public Outcome<ClaimValidationIssue> AcknowledgeIssue(AcknowledgeIssueInput input)
    {
        return AcknowledgeIssueOnClaimOperation.Run(this, input).OnSuccess(() => Settle(input));
    }

    public Outcome<IssueChanges> EvaluateIssues(EvaluateIssuesInput input)
    {
        return EvaluateIssuesOperation.Run(this, input).OnSuccess(changes => RecordRaisedIssues(changes.Raised));
    }

    public Outcome<Move<ClaimState>> Open(ClaimTransitionInput input)
    {
        Settle(input);

        return OpenClaimTransition.Run(this, input)
            .OnSuccess(move => RecordMove(move, input))
            .OnSuccess(() => Settle(input));
    }

    public Outcome<Move<ClaimState>> Investigate(ClaimTransitionInput input)
    {
        Settle(input);

        return InvestigateClaimTransition.Run(this, input)
            .OnSuccess(move => RecordMove(move, input))
            .OnSuccess(() => Settle(input));
    }

    public Outcome<Move<ClaimState>> Withdraw(FreezeClaimInput input)
    {
        Settle(input);

        return WithdrawClaimTransition.Run(this, input)
            .OnSuccess(move => RecordMove(move, input))
            .OnSuccess(() => Settle(input));
    }

    public Outcome<Move<ClaimState>> SendToPayment(SendToPaymentClaimInput input)
    {
        Settle(input);

        return SendToPaymentClaimTransition.Run(this, input)
            .OnSuccess(move => RecordMove(move, input))
            .OnSuccess(() => Settle(input));
    }

    public Outcome<Move<ClaimState>> Close(FreezeClaimInput input)
    {
        Settle(input);

        return CloseClaimTransition.Run(this, input)
            .OnSuccess(move => RecordMove(move, input))
            .OnSuccess(() => Settle(input));
    }

    public Outcome<Move<ClaimState>> Reopen(ReopenClaimInput input)
    {
        Settle(input);

        return ReopenClaimTransition.Run(this, input)
            .OnSuccess(move => RecordMove(move, input))
            .OnSuccess(() => OpenAfterReopen(input))
            .OnSuccess(() => Settle(input));
    }

    public static bool IsWithinReserveLimit(decimal reserveTotal)
    {
        bool isWithinLimit = reserveTotal <= ReserveLimit;

        return isWithinLimit;
    }

    public static bool IsReserveChangeWithinLimit(decimal approvedTotal, decimal amount)
    {
        bool isWithinLimit = amount <= 0m || IsWithinReserveLimit(approvedTotal + amount);

        return isWithinLimit;
    }

    public bool IsOnlyActiveClaimant(Guid partyId)
    {
        var activeClaimantIds = _parties
            .Where(party => party.IsActiveClaimant)
            .Select(party => party.Id);
        bool isOnlyActiveClaimant = activeClaimantIds.SequenceEqual([partyId]);

        return isOnlyActiveClaimant;
    }

    public bool HasOtherPrimaryRiskObject(Guid riskObjectId)
    {
        bool hasOtherPrimary = _riskObjects
            .Where(riskObject => !riskObject.IsDeleted)
            .Where(riskObject => riskObject.Id != riskObjectId)
            .Any(riskObject => riskObject.Details.IsPrimary);

        return hasOtherPrimary;
    }

    public bool HasWaivedFinding(Finding finding)
    {
        bool hasWaivedFinding = _issues
            .Where(issue => issue.State.IsWaived)
            .Any(issue => issue.Finding.IsSameProblem(finding));

        return hasWaivedFinding;
    }

    public bool HasUnresolvedWarning(string code)
    {
        bool hasUnresolvedWarning = _issues
            .Where(issue => issue.IsUnresolvedWarning)
            .Any(issue => issue.Finding.HasCode(code));

        return hasUnresolvedWarning;
    }

    private void Settle(ClaimBaseOperationInput input)
    {
        var evaluation = new EvaluateIssuesInput(input.LossReference);
        EvaluateIssues(evaluation);
    }

    private void RecordMove(Move<ClaimState> move, ClaimTransitionInput input)
    {
        Raise(new ClaimStateChangedEvent(Id, move.From.Name, move.To.Name, input.Reason));
    }

    private void OpenAfterReopen(ReopenClaimInput input)
    {
        var open = new ClaimTransitionInput(input.LossReference);
        OpenClaimTransition.Run(this, open).OnSuccess(move => RecordMove(move, open));
    }

    private void RecordRaisedIssues(IReadOnlyList<ClaimValidationIssue> raised)
    {
        foreach (var issue in raised)
        {
            Raise(new ValidationIssueRaisedEvent(Id, issue.Id, issue.Finding.Code, issue.Finding.Severity.ToString()));
        }
    }
}
