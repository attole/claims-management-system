using ClaimsModule.Domain.Common.Capabilities;
using ClaimsModule.Domain.Common.Entities;
using ClaimsModule.Domain.Core.Flows;
using ClaimsModule.Domain.Reserves.Flows;
using ClaimsModule.Domain.Reserves.Inputs;

namespace ClaimsModule.Domain.Reserves;

public sealed class Reserve : AggregateRoot, ISoftDeletable
{
    private Reserve()
    {
        ComponentType = new ReserveIndemnityComponentType();
    }

    internal Reserve(Guid id, Guid organisationId, Guid claimId, ReserveComponentType componentType, string notes)
        : base(id)
    {
        OrganisationId = organisationId;
        ClaimId = claimId;
        ComponentType = componentType;
        Notes = notes;
    }

    public Guid ClaimId { get; private set; } = Guid.Empty;

    public ReserveComponentType ComponentType { get; private set; }

    public ReserveStatus Status { get; private set; } = ReserveStatus.Active;

    public string Notes { get; private set; } = string.Empty;

    public ReserveReference Reference => new(Id, ClaimId, OrganisationId, ComponentType);

    public bool IsDeleted { get; private set; } = false;

    public DateTimeOffset DeletedAt { get; private set; } = DateTimeOffset.MinValue;

    public static Outcome<Reserve> Open(OpenReserveInput input)
    {
        return OpenReserveOperation.Run(input);
    }
}
