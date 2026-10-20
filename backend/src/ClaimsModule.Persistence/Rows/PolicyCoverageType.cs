using ClaimsModule.Domain.Common.Entities;

namespace ClaimsModule.Persistence.Rows;

internal sealed class PolicyCoverageType : Entity
{
    private PolicyCoverageType()
    {
    }

    public PolicyCoverageType(Guid id, Guid policyId, Guid organisationId, string name)
        : base(id)
    {
        PolicyId = policyId;
        OrganisationId = organisationId;
        Name = name;
    }

    public Guid PolicyId { get; private set; } = Guid.Empty;

    public string Name { get; private set; } = string.Empty;
}
