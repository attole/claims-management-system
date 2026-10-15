namespace ClaimsModule.Domain.ReserveChanges.Root;

public sealed record ReserveReview(Guid ReviewedBy, DateTimeOffset ReviewedAt, string Reason)
{
    public static ReserveReview Empty { get; } = new(Guid.Empty, DateTimeOffset.MinValue, string.Empty);

    public bool IsSet => ReviewedBy != Guid.Empty;
}
