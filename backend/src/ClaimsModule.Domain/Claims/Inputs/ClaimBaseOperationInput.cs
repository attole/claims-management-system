using ClaimsModule.Domain.CauseOfLoss;
using ClaimsModule.Domain.Common;
using ClaimsModule.Domain.Policies;

namespace ClaimsModule.Domain.Claims.Inputs;

public abstract record ClaimBaseOperationInput(PolicyPeriod PolicyPeriod, CauseCodes CauseCodes, DateTimeOffset Now)
{
    public DateTimeOffset Now { get; } = InputTime.Required(Now);
}
