using ClaimsModule.Domain.Common.Capabilities;
using ClaimsModule.Domain.Common.Entities;

namespace ClaimsModule.Persistence.Tests.Support.Conventions;

internal sealed class SoftDeletableRow : Entity, ISoftDeletable
{
    private SoftDeletableRow()
    {
    }

    public SoftDeletableRow(Guid id, Guid organisationId)
        : base(id)
    {
        OrganisationId = organisationId;
    }

    public bool IsDeleted { get; private set; } = false;

    public void Remove()
    {
        IsDeleted = true;
    }
}
