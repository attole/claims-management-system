namespace ClaimsModule.Application.Claims.Loss;

public sealed record LossRequest(DateTimeOffset LossDate, string Description, string CauseOfLossCode)
{
    public string Location { get; init; } = string.Empty;

    public decimal? EstimatedLossAmount { get; init; }

    public string PoliceReportNumber { get; init; } = string.Empty;
}
