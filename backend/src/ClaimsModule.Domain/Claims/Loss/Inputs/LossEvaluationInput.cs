using ClaimsModule.Domain.Claims.Issues;

namespace ClaimsModule.Domain.Claims.Loss.Inputs;

internal sealed record LossEvaluationInput(
    DateTimeOffset Now,
    DateTimeOffset ReportDate,
    ClaimReferenceSnapshot Reference);
