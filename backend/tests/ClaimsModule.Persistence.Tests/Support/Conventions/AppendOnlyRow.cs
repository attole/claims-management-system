using ClaimsModule.Domain.Common.Entities;

namespace ClaimsModule.Persistence.Tests.Support.Conventions;

internal sealed class AppendOnlyRow : Entity
{
    private AppendOnlyRow()
    {
    }

    public AppendOnlyRow(Guid id, Guid organisationId)
        : base(id)
    {
        OrganisationId = organisationId;
    }
}
