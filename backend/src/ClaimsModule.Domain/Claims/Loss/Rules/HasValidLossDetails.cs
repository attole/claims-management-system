using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.Claims.Loss.Rules;

internal sealed class HasValidLossDetails(DateOnly reportDay, LossReference reference) : IRule<LossDetails>
{
    public const string Code = "LOSS_DETAILS_VALID";

    public CheckResult Check(LossDetails loss)
    {
        bool isActiveCauseCode = reference.IsActiveCauseCode(loss.CauseOfLossCode);
        CheckResult[] results =
        [
            new IsLossDatePresent().Check(loss),
            new IsLossDateNotAfterReport(reportDay).Check(loss),
            new IsDescriptionLongEnough().Check(loss),
            new IsCauseCodeActive(isActiveCauseCode).Check(loss),
            new IsLossDateWithinPolicyPeriod(reference.PolicyPeriod).Check(loss),
        ];

        return CheckResult.Combine(results);
    }
}
