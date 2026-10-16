using ClaimsModule.Domain.Claims.Inputs;
using ClaimsModule.Domain.Claims.Loss;

namespace ClaimsModule.Domain.Claims.Parties.Inputs;

public sealed record AddPartyInput(PartyDetails Details, LossReference LossReference)
    : ClaimBaseOperationInput(LossReference);
