using ClaimsModule.Domain.Common.Capabilities;
using ClaimsModule.Domain.Common.Entities;

namespace ClaimsModule.Domain.Policies;

public sealed class Policy : Entity, ISoftDeletable
{
    private Policy()
    {
        Period = PolicyPeriod.Empty;
    }

    internal Policy(
        Guid id,
        Guid organisationId,
        string policyNumber,
        string clientName,
        PolicyPeriod period,
        IReadOnlyList<string> coverageTypes)
        : base(id)
    {
        OrganisationId = organisationId;
        PolicyNumber = policyNumber;
        ClientName = clientName;
        Period = period;
        CoverageTypes = coverageTypes;
    }

    public string PolicyNumber { get; private set; } = string.Empty;

    public string ClientName { get; private set; } = string.Empty;

    public PolicyPeriod Period { get; private set; }

    public IReadOnlyList<string> CoverageTypes { get; private set; } = [];

    public bool IsDeleted { get; private set; } = false;
}
