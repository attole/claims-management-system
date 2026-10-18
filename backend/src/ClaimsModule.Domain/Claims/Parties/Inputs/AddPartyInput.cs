using ClaimsModule.Domain.Claims.Inputs;
using ClaimsModule.Domain.Claims.Loss;
using ClaimsModule.Domain.Policies;

namespace ClaimsModule.Domain.Claims.Parties.Inputs;

public sealed record AddPartyInput(PartyDetails Details, PolicyPeriod PolicyPeriod, CauseCodes CauseCodes)
    : ClaimBaseOperationInput(PolicyPeriod, CauseCodes);
