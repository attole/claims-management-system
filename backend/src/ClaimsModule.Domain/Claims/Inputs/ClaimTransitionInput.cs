using ClaimsModule.Domain.Claims.Loss;
using ClaimsModule.Domain.Claims.StateChanges;

namespace ClaimsModule.Domain.Claims.Inputs;

public record ClaimTransitionInput(LossReference LossReference) : ClaimBaseOperationInput(LossReference)
{
    public StateChangeReason Reason { get; init; } = StateChangeReason.Empty;

    public string Note { get; init; } = string.Empty;

    public bool HasNote => !string.IsNullOrWhiteSpace(Note);
}
