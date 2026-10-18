namespace ClaimsModule.Domain.ReserveComponents.History;

public sealed record ReserveReview(Guid ReviewedBy, DateTimeOffset ReviewedAt, string Reason)
{
    public static ReserveReview Empty { get; } = new(Guid.Empty, DateTimeOffset.MinValue, string.Empty);

    public bool IsSet => ReviewedAt != DateTimeOffset.MinValue;
}
