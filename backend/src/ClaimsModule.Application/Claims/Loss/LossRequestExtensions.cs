using ClaimsModule.Domain.Claims.Loss;

namespace ClaimsModule.Application.Claims.Loss;

public static class LossRequestExtensions
{
    public static LossDetails ToDetails(this LossRequest request)
    {
        var lossDate = new LossDate(request.LossDate);

        return new LossDetails(lossDate, request.Description, request.CauseOfLossCode)
        {
            Location = request.Location ?? string.Empty,
            EstimatedLossAmount = ToEstimate(request.EstimatedLossAmount),
            PoliceReportNumber = request.PoliceReportNumber ?? string.Empty,
        };
    }

    private static EstimatedLossAmount ToEstimate(decimal? amount)
    {
        return amount.HasValue ? new EstimatedLossAmount(amount.Value) : EstimatedLossAmount.Empty;
    }
}
