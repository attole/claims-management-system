using ClaimsModule.Domain.CauseOfLoss;
using ClaimsModule.Domain.Policies;

namespace ClaimsModule.Domain.Claims.Inputs;

public sealed record UpdateNotesInput(
    string Notes,
    PolicyPeriod PolicyPeriod,
    CauseCodes CauseCodes,
    DateTimeOffset Now) : ClaimBaseOperationInput(PolicyPeriod, CauseCodes, Now);
