using ClaimsModule.Domain.Claims.Loss;
using LossEstimate = ClaimsModule.Domain.Claims.Loss.EstimatedLossAmount;

namespace ClaimsModule.Application.Claims.Loss;

public sealed record LossRequest(DateTimeOffset LossDate, string Description, string CauseOfLossCode)
{
    public string Location { get; init; } = string.Empty;

    public decimal? EstimatedLossAmount { get; init; }

    public string PoliceReportNumber { get; init; } = string.Empty;

    public LossDetails ToDetails()
    {
        var lossDate = new LossDate(LossDate);
        var estimate = EstimatedLossAmount.HasValue ? new LossEstimate(EstimatedLossAmount.Value) : LossEstimate.Empty;

        return new LossDetails(lossDate, Description, CauseOfLossCode)
        {
            Location = Location ?? string.Empty,
            EstimatedLossAmount = estimate,
            PoliceReportNumber = PoliceReportNumber ?? string.Empty,
        };
    }
}
