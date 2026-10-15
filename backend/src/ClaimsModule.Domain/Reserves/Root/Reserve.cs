using ClaimsModule.Domain.Common.Capabilities;
using ClaimsModule.Domain.Common.Entities;
using ClaimsModule.Domain.Core.Base.Flows;
using ClaimsModule.Domain.Core.Rules;
using ClaimsModule.Domain.Reserves.Root.Operations;

namespace ClaimsModule.Domain.Reserves.Root;

public sealed class Reserve : AggregateRoot, IAuditable, ISoftDeletable, ITenantOwned
{
    private Reserve()
    {
        ComponentType = new ReserveIndemnityComponentType();
    }

    internal Reserve(
        Guid id,
        Guid organisationId,
        Guid claimId,
        ReserveComponentType componentType,
        string notes,
        DateTimeOffset now)
        : base(id)
    {
        OrganisationId = organisationId;
        ClaimId = claimId;
        ComponentType = componentType;
        Notes = notes;
        CreatedAt = now;
        UpdatedAt = now;
    }

    public Guid ClaimId { get; private set; } = Guid.Empty;

    public ReserveComponentType ComponentType { get; private set; }

    public ReserveStatus Status { get; private set; } = ReserveStatus.Active;

    public string Notes { get; private set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.MinValue;

    public DateTimeOffset UpdatedAt { get; private set; } = DateTimeOffset.MinValue;

    public Guid UserCreated { get; private set; } = Guid.Empty;

    public Guid UserModified { get; private set; } = Guid.Empty;

    public bool IsDeleted { get; private set; } = false;

    public DateTimeOffset DeletedAt { get; private set; } = DateTimeOffset.MinValue;

    public Guid OrganisationId { get; private set; } = Guid.Empty;

    public static FlowOutcome<Reserve, DomainFinding> Create(OpenReserveInput input)
    {
        return new OpenReserveOperation().Execute(input);
    }
}
