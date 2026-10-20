using ClaimsModule.Domain.Common.Capabilities;
using ClaimsModule.Domain.Common.Entities;

namespace ClaimsModule.Domain.CauseOfLoss;

public sealed class CauseOfLossCode : Entity, ISoftDeletable
{
    private CauseOfLossCode()
    {
    }

    internal CauseOfLossCode(
        Guid id,
        Guid organisationId,
        string code,
        string name,
        PerilCategory perilCategory,
        bool isActive)
        : base(id)
    {
        OrganisationId = organisationId;
        Code = code;
        Name = name;
        PerilCategory = perilCategory;
        IsActive = isActive;
    }

    public string Code { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public PerilCategory PerilCategory { get; private set; } = PerilCategory.General;

    public bool IsActive { get; private set; } = false;

    public bool IsDeleted { get; private set; } = false;
}
