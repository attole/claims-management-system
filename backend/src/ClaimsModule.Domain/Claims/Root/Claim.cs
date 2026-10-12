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

    private Claim()
    {
        ClaimNumber = ClaimNumber.Empty;
        State = new DraftState();
        Severity = ClaimSeverity.Unspecified;
        LossEvent = new LossEvent(Guid.Empty, Guid.Empty, Guid.Empty, LossDetails.Empty, DateTimeOffset.MinValue);
    }

    private Claim(Guid id, Guid organisationId, ClaimNumber claimNumber, DateTimeOffset now)
        : base(id)
    {
        OrganisationId = organisationId;
        ClaimNumber = claimNumber;
        State = new DraftState();
        Severity = ClaimSeverity.Unspecified;
        ReportedDate = now;
        CreatedAt = now;
        UpdatedAt = now;
        LossEvent = new LossEvent(
            Ids.Next(now),
            id,
            organisationId,
            LossDetails.Empty,
            now);
    }

    public ClaimNumber ClaimNumber { get; private set; }

    public ClaimState State { get; private set; }

    public ClaimSeverity Severity { get; private set; }

    public DateTimeOffset ReportedDate { get; private set; }

    public Guid AssignedHandlerId { get; private set; } = Guid.Empty;

    public string Notes { get; private set; } = string.Empty;

    public bool AggregateLimitOverride { get; private set; } = false;

    public LossEvent LossEvent { get; private set; }

    public IReadOnlyList<ClaimParty> Parties => _parties;

    public IReadOnlyList<ClaimRiskObject> RiskObjects => _riskObjects
        .Where(item => !item.IsDeleted)
        .ToList();

    public int ActiveClaimantCount => _parties.Count(party => party.IsActiveClaimant);

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public Guid UserCreated { get; private set; } = Guid.Empty;

    public Guid UserModified { get; private set; } = Guid.Empty;

    public bool IsDeleted { get; private set; } = false;

    public DateTimeOffset DeletedAt { get; private set; } = DateTimeOffset.MinValue;

    public Guid OrganisationId { get; private set; }

    public static Claim Create(Guid organisationId, ClaimNumber claimNumber, Guid actorId, DateTimeOffset now)
    {
        EnsureCanCreate(organisationId, claimNumber, now);

        Claim claim = new(
            Ids.Next(now),
            organisationId,
            claimNumber,
            now);
        claim.Raise(new ClaimCreated(
            Ids.Next(now),
            now,
            actorId,
            claim.Id,
            claimNumber.Value));

        return claim;
    }

    internal ClaimParty AddParty(PartyDetails details, Guid actorId, DateTimeOffset now)
    {
        ClaimParty party = new(
            Ids.Next(now),
            Id,
            OrganisationId,
            details,
            now);
        _parties.Add(party);
        Touch(now);
        Raise(new PartyAdded(
            Ids.Next(now),
            now,
            actorId,
            Id,
            party.Id,
            party.Details.Role.Name));

        return party;
    }

    internal void DeactivateParty(Guid partyId, Guid actorId, DateTimeOffset now)
    {
        ClaimParty party = _parties.Single(candidate => candidate.Id == partyId && candidate.IsActive);
        party.Deactivate(now);
        Touch(now);
        Raise(new PartyRemoved(
            Ids.Next(now),
            now,
            actorId,
            Id,
            party.Id,
            party.Details.Role.Name));
    }

    internal ClaimRiskObject AddRiskObject(RiskObjectDetails details, DateTimeOffset now)
    {
        ClaimRiskObject riskObject = new(
            Ids.Next(now),
            Id,
            OrganisationId,
            details,
            now);
        _riskObjects.Add(riskObject);
        Touch(now);

        return riskObject;
    }

    internal void RemoveRiskObject(Guid riskObjectId, DateTimeOffset now)
    {
        ClaimRiskObject riskObject = _riskObjects
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

    private static void EnsureCanCreate(Guid organisationId, ClaimNumber claimNumber, DateTimeOffset now)
    {
        if (organisationId == Guid.Empty)
        {
            throw new ArgumentException("A claim needs an organisation.", nameof(organisationId));
        }

        if (claimNumber.Equals(ClaimNumber.Empty))
        {
            throw new ArgumentException("A claim needs a claim number.", nameof(claimNumber));
        }

        if (now == default)
        {
            throw new ArgumentException("A claim needs the time of its creation.", nameof(now));
        }
    }

    private void Touch(DateTimeOffset now)
    {
        UpdatedAt = now;
    }
}
