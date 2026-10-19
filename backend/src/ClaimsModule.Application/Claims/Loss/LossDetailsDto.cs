namespace ClaimsModule.Application.Claims.Loss;

public sealed record LossDetailsDto
{
    public Guid Id { get; init; } = Guid.Empty;

    public Guid ClaimId { get; init; } = Guid.Empty;

    public DateTimeOffset ReportDate { get; init; } = DateTimeOffset.MinValue;

    public DateTimeOffset LossDate { get; init; } = DateTimeOffset.MinValue;

    public string Description { get; init; } = string.Empty;

    public string CauseOfLossCode { get; init; } = string.Empty;

    public string Location { get; init; } = string.Empty;

    public bool HasEstimatedLossAmount { get; init; } = false;

    public decimal EstimatedLossAmount { get; init; } = 0m;

    public string PoliceReportNumber { get; init; } = string.Empty;
}
