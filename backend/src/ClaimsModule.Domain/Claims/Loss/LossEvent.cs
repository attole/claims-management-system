using ClaimsModule.Domain.Claims.Loss.Flows;
using ClaimsModule.Domain.Claims.Loss.Inputs;
using ClaimsModule.Domain.Common.Capabilities;
using ClaimsModule.Domain.Common.Entities;
using ClaimsModule.Domain.Core.Flows;

namespace ClaimsModule.Domain.Claims.Loss;

public sealed class LossEvent : Entity, ISoftDeletable
{
    private LossEvent()
    {
        Details = LossDetails.Empty;
    }

    internal LossEvent(Guid id, Guid claimId, Guid organisationId, LossDetails details, DateTimeOffset reportDate)
        : base(id)
    {
        ClaimId = claimId;
        OrganisationId = organisationId;
        ReportDate = reportDate;
        Details = details;
    }

    public Guid ClaimId { get; private set; } = Guid.Empty;

    public LossDetails Details { get; internal set; }

    public DateTimeOffset ReportDate { get; private set; } = DateTimeOffset.MinValue;

    public bool IsDeleted { get; private set; } = false;

    public DateTimeOffset DeletedAt { get; private set; } = DateTimeOffset.MinValue;

    internal Outcome<LossEvent> Update(LossDetailsInput input)
    {
        return UpdateLossDetailsOperation.Run(this, input);
    }
}
