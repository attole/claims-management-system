using ClaimsModule.Domain.Claims.Loss;
using FluentValidation;
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

public sealed class LossRequestValidator : AbstractValidator<LossRequest>
{
    public LossRequestValidator()
    {
        RuleFor(request => request.Description).NotNull();
        RuleFor(request => request.CauseOfLossCode).NotNull();
        RuleFor(request => request.EstimatedLossAmount)
            .GreaterThanOrEqualTo(0m)
            .When(request => request.EstimatedLossAmount.HasValue);
        RuleFor(request => request.CauseOfLossCode).MaximumLength(ClaimFieldLengths.CauseOfLossCode);
        RuleFor(request => request.Location).MaximumLength(ClaimFieldLengths.Location);
        RuleFor(request => request.PoliceReportNumber).MaximumLength(ClaimFieldLengths.PoliceReportNumber);
    }
}
