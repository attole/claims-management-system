using System.Globalization;
using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.Claims.Loss.Rules;

internal sealed class IsLossDateWithinPolicyPeriod(PolicyPeriod period) : IRule<LossDetails>
{
    public const string Code = "S:BR-C-02";

    private const string Message = "Loss date is outside the policy effective period.";
    private const string Field = "LossDate";

    public CheckResult Check(LossDetails loss)
    {
        bool isWithinPeriod = period.CanCoverLoss(loss.LossDate);
        string evidence = string.Join(
            '|',
            loss.LossDate.UtcDay.ToString("O", CultureInfo.InvariantCulture),
            period.StartDate.ToString("O", CultureInfo.InvariantCulture),
            period.EffectiveEndDate.ToString("O", CultureInfo.InvariantCulture));
        var warning = new Finding(Code, Message, Field) { Evidence = evidence }.AsWarning();

        return isWithinPeriod
            ? CheckResult.Pass
            : CheckResult.Found(warning);
    }
}
