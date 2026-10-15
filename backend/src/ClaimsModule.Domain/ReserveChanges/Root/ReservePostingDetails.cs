namespace ClaimsModule.Domain.ReserveChanges.Root;

public sealed record ReservePostingDetails(string JobId, string FailureReason)
{
    public static ReservePostingDetails Empty { get; } = new(string.Empty, string.Empty);

    public bool IsSet => !string.IsNullOrEmpty(JobId) || !string.IsNullOrEmpty(FailureReason);
}
