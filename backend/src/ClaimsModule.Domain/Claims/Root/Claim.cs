using ClaimsModule.Domain.Claims.Issues;
using ClaimsModule.Domain.Claims.Issues.Events;
using ClaimsModule.Domain.Claims.Loss;
using ClaimsModule.Domain.Claims.Parties;
using ClaimsModule.Domain.Claims.Parties.Events;
using ClaimsModule.Domain.Claims.RiskObjects;
using ClaimsModule.Domain.Claims.Root.Events;
using ClaimsModule.Domain.Common;
using ClaimsModule.Domain.Common.Capabilities;
using ClaimsModule.Domain.Common.Entities;

namespace ClaimsModule.Domain.Claims.Root;

public sealed class Claim : AggregateRoot, IAuditable, ISoftDeletable, ITenantOwned
{
    private readonly List<ClaimParty> _parties = [];
    private readonly List<ClaimRiskObject> _riskObjects = [];
    private readonly List<ClaimValidationIssue> _issues = [];

    private Claim()
    {
        ClaimNumber = ClaimNumber.Empty;
        State = new ClaimDraftState();
        Severity = ClaimSeverity.Unspecified;
        LossEvent = new LossEvent(Guid.Empty, Guid.Empty, Guid.Empty, LossDetails.Empty, DateTimeOffset.MinValue);
    }

    private Claim(Guid id, Guid organisationId, ClaimNumber claimNumber, DateTimeOffset now)
        : base(id)
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

    public ClaimState State { get; private set; }

    public ClaimSeverity Severity { get; private set; }

    public DateTimeOffset ReportedDate { get; private set; } = DateTimeOffset.MinValue;

    public Guid AssignedHandlerId { get; private set; } = Guid.Empty;

    public string Notes { get; private set; } = string.Empty;

    public bool HasAggregateLimitOverride { get; private set; } = false;

    public LossEvent LossEvent { get; private set; }

    public IReadOnlyList<ClaimParty> Parties => _parties.AsReadOnly();

    public IReadOnlyList<ClaimRiskObject> RiskObjects => _riskObjects
        .Where(riskObject => !riskObject.IsDeleted)
        .ToList();

    public IReadOnlyList<ClaimValidationIssue> Issues => _issues.AsReadOnly();

    public IReadOnlyList<ClaimValidationIssue> BlockingIssues => _issues
        .Where(issue => issue.IsUnresolvedCritical)
        .ToList();

    public int ActiveClaimantCount => _parties.Count(party => party.IsActiveClaimant);

    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.MinValue;

    public DateTimeOffset UpdatedAt { get; private set; } = DateTimeOffset.MinValue;

    public Guid UserCreated { get; private set; } = Guid.Empty;

    public Guid UserModified { get; private set; } = Guid.Empty;

    public bool IsDeleted { get; private set; } = false;

    public DateTimeOffset DeletedAt { get; private set; } = DateTimeOffset.MinValue;

    public Guid OrganisationId { get; private set; } = Guid.Empty;

    internal static Claim Create(Guid organisationId, ClaimNumber claimNumber, Guid actorId, DateTimeOffset now)
    {
        var claim = new Claim(Ids.Next(now), organisationId, claimNumber, now);
        claim.Raise(new ClaimCreated(Ids.Next(now), now, actorId, claim.Id, claimNumber.Value));

        return claim;
    }

    internal ClaimParty AddParty(PartyDetails details, Guid actorId, DateTimeOffset now)
    {
        var party = new ClaimParty(Ids.Next(now), Id, OrganisationId, details, now);
        _parties.Add(party);
        Touch(now);
        Raise(new PartyAdded(Ids.Next(now), now, actorId, Id, party.Id, party.Details.Role.Name));

        return party;
    }

    internal void DeactivateParty(Guid partyId, Guid actorId, DateTimeOffset now)
    {
        var party = _parties.Single(candidate => candidate.Id == partyId && candidate.IsActive);
        party.Deactivate(now);
        Touch(now);
        Raise(new PartyRemoved(Ids.Next(now), now, actorId, Id, party.Id, party.Details.Role.Name));
    }

    internal ClaimRiskObject AddRiskObject(RiskObjectDetails details, DateTimeOffset now)
    {
        var riskObject = new ClaimRiskObject(Ids.Next(now), Id, OrganisationId, details, now);
        _riskObjects.Add(riskObject);
        Touch(now);

        return riskObject;
    }

    internal void RemoveRiskObject(Guid riskObjectId, DateTimeOffset now)
    {
        var riskObject = _riskObjects
            .Single(candidate => candidate.Id == riskObjectId && !candidate.IsDeleted);
        riskObject.Remove(now);
        Touch(now);
    }

    internal void UpdateLossDetails(LossDetails details, DateTimeOffset now)
    {
        LossEvent.Update(details, now);
        Touch(now);
    }

    internal void AssignHandler(Guid handlerId, DateTimeOffset now)
    {
        AssignedHandlerId = handlerId;
        Touch(now);
    }

    internal void SetSeverity(ClaimSeverity severity, DateTimeOffset now)
    {
        Severity = severity;
        Touch(now);
    }

    internal void UpdateNotes(string notes, DateTimeOffset now)
    {
        Notes = notes;
        Touch(now);
    }

    internal void ChangeState(ClaimState state, Guid actorId, DateTimeOffset now)
    {
        string from = State.Name;
        State = state;
        Touch(now);
        Raise(new ClaimStatusChanged(Ids.Next(now), now, actorId, Id, from, state.Name));
    }

    internal void WaiveIssue(Guid issueId, Guid actorId, DateTimeOffset now)
    {
        var issue = _issues.Single(candidate => candidate.Id == issueId);
        issue.Waive(actorId, now);
        Touch(now);
        Raise(new ValidationIssueWaived(Ids.Next(now), now, actorId, Id, issue.Id, issue.Finding.Code));
    }

    internal void ClearIssue(ClaimValidationIssue issue, Guid actorId, DateTimeOffset now)
    {
        issue.Clear(actorId, now);
        Touch(now);
        Raise(new ValidationIssueCleared(Ids.Next(now), now, actorId, Id, issue.Id, issue.Finding.Code));
    }

    internal void RaiseIssue(IssueFinding finding, Guid actorId, DateTimeOffset now)
    {
        var issue = new ClaimValidationIssue(Ids.Next(now), Id, OrganisationId, finding, now);
        _issues.Add(issue);
        Touch(now);
        var eventId = Ids.Next(now);
        string severity = finding.Severity.Name;
        Raise(new ValidationIssueRaised(eventId, now, actorId, Id, issue.Id, finding.Code, severity));
    }

    private void Touch(DateTimeOffset now)
    {
        UpdatedAt = now;
    }
}
