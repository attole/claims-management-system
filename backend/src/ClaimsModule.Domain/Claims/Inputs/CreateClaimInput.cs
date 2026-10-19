using ClaimsModule.Domain.CauseOfLoss;
using ClaimsModule.Domain.Common;
using ClaimsModule.Domain.Policies;

namespace ClaimsModule.Domain.Claims.Inputs;

public sealed record CreateClaimInput(
    Guid OrganisationId,
    ClaimNumber ClaimNumber,
    DateTimeOffset ReportDate,
    DateTimeOffset Now,
    PolicyPeriod PolicyPeriod,
    CauseCodes CauseCodes) : ClaimBaseOperationInput(PolicyPeriod, CauseCodes)
{
    public DateTimeOffset Now { get; } = InputTime.Required(Now);
}
