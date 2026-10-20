using ClaimsModule.Domain.Claims.RiskObjects.Flows;
using ClaimsModule.Domain.Claims.RiskObjects.Inputs;
using ClaimsModule.Domain.Common.Capabilities;
using ClaimsModule.Domain.Common.Entities;
using ClaimsModule.Domain.Core.Flows;

namespace ClaimsModule.Domain.Claims.RiskObjects;

public sealed class ClaimRiskObject : Entity, ISoftDeletable
{
    private ClaimRiskObject()
    {
        Details = new RiskObjectDetails(AssetType.Other, string.Empty);
    }

    internal ClaimRiskObject(Guid id, Guid claimId, Guid organisationId, RiskObjectDetails details) : base(id)
    {
        ClaimId = claimId;
        OrganisationId = organisationId;
        Details = details;
    }

    public Guid ClaimId { get; private set; } = Guid.Empty;

    public RiskObjectDetails Details { get; internal set; }

    public bool IsDeleted { get; internal set; } = false;

    internal Outcome<ClaimRiskObject> Update(RiskObjectDetailsInput input)
    {
        return UpdateRiskObjectOperation.Run(this, input);
    }

    internal Outcome<ClaimRiskObject> Remove()
    {
        return RemoveRiskObjectOperation.Run(this, NoInput.Value);
    }
}
