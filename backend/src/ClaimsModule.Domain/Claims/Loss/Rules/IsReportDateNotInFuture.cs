using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.Claims.Loss.Rules;

internal sealed class IsReportDateNotInFuture(DateTimeOffset reportDate, DateTimeOffset now) : IRule<object>
{
    public const string Code = "LOSS_REPORT_DATE_IN_FUTURE";

    private const string Message = "The report date cannot be later than the time the claim is created.";
    private const string Field = "ReportDate";

    public CheckResult Check(object subject)
    {
        bool isNotInFuture = reportDate <= now;

        return isNotInFuture
            ? CheckResult.Pass
            : CheckResult.Found(new Finding(Code, Message, Field));
    }
}
