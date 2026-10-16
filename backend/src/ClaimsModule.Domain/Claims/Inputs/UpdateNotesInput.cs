using ClaimsModule.Domain.Claims.Loss;

namespace ClaimsModule.Domain.Claims.Inputs;

public sealed record UpdateNotesInput(string Notes, LossReference LossReference)
    : ClaimBaseOperationInput(LossReference);
