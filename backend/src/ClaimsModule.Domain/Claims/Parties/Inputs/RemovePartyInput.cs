using ClaimsModule.Domain.Claims.Inputs;
using ClaimsModule.Domain.Claims.Loss;

namespace ClaimsModule.Domain.Claims.Parties.Inputs;

public sealed record RemovePartyInput(Guid PartyId, LossReference LossReference)
    : ClaimBaseOperationInput(LossReference);
