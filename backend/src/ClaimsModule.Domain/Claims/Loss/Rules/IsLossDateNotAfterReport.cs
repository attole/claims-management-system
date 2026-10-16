using System.Globalization;
using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.Claims.Loss.Rules;

internal sealed class IsLossDateNotAfterReport(DateOnly reportDay) : IRule<LossDetails>
{
    public const string Code = "LOSS_DATE_AFTER_REPORT";

    private const string Message = "Loss date cannot be after the date the claim was reported.";
    private const string Field = "LossDate";

    public CheckResult Check(LossDetails loss)
    {
        bool isNotAfterReport = !loss.LossDate.IsAfterDay(reportDay);
        string evidence = string.Join(
            '|',
            loss.LossDate.UtcDay.ToString("O", CultureInfo.InvariantCulture),
            reportDay.ToString("O", CultureInfo.InvariantCulture));

        return isNotAfterReport
            ? CheckResult.Pass
            : CheckResult.Found(new Finding(Code, Message, Field) { Evidence = evidence });
    }
}
