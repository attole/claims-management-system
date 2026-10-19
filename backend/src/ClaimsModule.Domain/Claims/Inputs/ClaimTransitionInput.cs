using ClaimsModule.Domain.CauseOfLoss;
using ClaimsModule.Domain.Claims.StateChanges;
using ClaimsModule.Domain.Policies;

namespace ClaimsModule.Domain.Claims.Inputs;

public record ClaimTransitionInput(PolicyPeriod PolicyPeriod, CauseCodes CauseCodes)
    : ClaimBaseOperationInput(PolicyPeriod, CauseCodes)
{
    public StateChangeReason Reason { get; init; } = StateChangeReason.Empty;

    public string Note { get; init; } = string.Empty;

    public bool HasNote => !string.IsNullOrWhiteSpace(Note);
}
