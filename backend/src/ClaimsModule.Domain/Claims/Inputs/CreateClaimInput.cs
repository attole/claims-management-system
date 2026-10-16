using ClaimsModule.Domain.Claims.Loss;
using ClaimsModule.Domain.Common;

namespace ClaimsModule.Domain.Claims.Inputs;

public sealed record CreateClaimInput(
    Guid OrganisationId,
    ClaimNumber ClaimNumber,
    DateTimeOffset ReportDate,
    DateTimeOffset Now,
    LossReference LossReference) : ClaimBaseOperationInput(LossReference)
{
    public DateTimeOffset Now { get; } = InputTime.Required(Now);
}
