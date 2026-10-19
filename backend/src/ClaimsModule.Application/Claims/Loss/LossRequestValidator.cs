using FluentValidation;

namespace ClaimsModule.Application.Claims.Loss;

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
