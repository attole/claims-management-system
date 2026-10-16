namespace ClaimsModule.Domain.ReserveChanges.Inputs;

public sealed record ReservePostingInput
{
    public string JobId { get; init; } = string.Empty;

    public string FailureReason { get; init; } = string.Empty;
}
