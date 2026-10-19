using ClaimsModule.Domain.CauseOfLoss;
using ClaimsModule.Domain.Claims.Inputs;
using ClaimsModule.Domain.Policies;

namespace ClaimsModule.Domain.Claims.Parties.Inputs;

public sealed record RemovePartyInput(Guid PartyId, PolicyPeriod PolicyPeriod, CauseCodes CauseCodes)
    : ClaimBaseOperationInput(PolicyPeriod, CauseCodes);
