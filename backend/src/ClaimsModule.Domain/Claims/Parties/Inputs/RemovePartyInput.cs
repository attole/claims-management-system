using ClaimsModule.Domain.Claims.Inputs;
using ClaimsModule.Domain.Claims.Loss;
using ClaimsModule.Domain.Policies;

namespace ClaimsModule.Domain.Claims.Parties.Inputs;

public sealed record RemovePartyInput(Guid PartyId, PolicyPeriod PolicyPeriod, CauseCodes CauseCodes)
    : ClaimBaseOperationInput(PolicyPeriod, CauseCodes);
