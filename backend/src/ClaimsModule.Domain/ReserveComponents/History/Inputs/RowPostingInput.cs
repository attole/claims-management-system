namespace ClaimsModule.Domain.ReserveComponents.History.Inputs;

internal sealed record RowPostingInput
{
    public string JobId { get; init; } = string.Empty;

    public string FailureReason { get; init; } = string.Empty;
}
