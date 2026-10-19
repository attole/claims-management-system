namespace ClaimsModule.Application.ReserveComponents.History;

public sealed record ReserveHistoryDto
{
    public Guid Id { get; init; } = Guid.Empty;

    public Guid ReserveComponentId { get; init; } = Guid.Empty;

    public Guid ClaimId { get; init; } = Guid.Empty;

    public string ComponentType { get; init; } = string.Empty;

    public string TransactionType { get; init; } = string.Empty;

    public decimal Amount { get; init; } = 0m;

    public string ChangeReason { get; init; } = string.Empty;

    public int ChangeSequence { get; init; } = 0;

    public string ApprovalStatus { get; init; } = string.Empty;

    public string PostingStatus { get; init; } = string.Empty;

    public Guid SubmittedByUserId { get; init; } = Guid.Empty;

    public DateTimeOffset SubmittedAt { get; init; } = DateTimeOffset.MinValue;

    public Guid ReviewedBy { get; init; } = Guid.Empty;

    public DateTimeOffset ReviewedAt { get; init; } = DateTimeOffset.MinValue;

    public string ReviewReason { get; init; } = string.Empty;

    public string JobId { get; init; } = string.Empty;

    public string FailureReason { get; init; } = string.Empty;

    public string IdempotencyKey { get; init; } = string.Empty;
}
