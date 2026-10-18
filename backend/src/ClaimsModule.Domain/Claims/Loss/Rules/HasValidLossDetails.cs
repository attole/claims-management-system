using ClaimsModule.Domain.Core.Rules;
using ClaimsModule.Domain.Policies;

namespace ClaimsModule.Domain.Claims.Loss.Rules;

internal sealed class HasValidLossDetails(DateOnly reportDay, PolicyPeriod period, CauseCodes causeCodes)
    : IRule<LossDetails>
{
    public const string Code = "LOSS_DETAILS_VALID";

    public CheckResult Check(LossDetails loss)
    {
        bool isActiveCauseCode = causeCodes.IsActive(loss.CauseOfLossCode);
        CheckResult[] results =
        [
            new IsLossDatePresent().Check(loss),
            new IsLossDateNotAfterReport(reportDay).Check(loss),
            new IsDescriptionLongEnough().Check(loss),
            new IsCauseCodeActive(isActiveCauseCode).Check(loss),
            new IsLossDateWithinPolicyPeriod(period).Check(loss),
        ];

        return CheckResult.Combine(results);
    }
}
