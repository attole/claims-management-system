namespace ClaimsModule.Domain.ReserveComponents.History.Inputs;

public sealed record HistoryPostingInput(Guid ReserveHistoryId)
{
    public string JobId { get; init; } = string.Empty;

    public string FailureReason { get; init; } = string.Empty;
}
