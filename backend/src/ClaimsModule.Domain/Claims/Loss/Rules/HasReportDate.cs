using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.Claims.Loss.Rules;

internal sealed class HasReportDate(DateTimeOffset reportDate) : IRule<object>
{
    public const string Code = "LOSS_REPORT_DATE_REQUIRED";

    private const string Message = "A claim needs the date its loss was reported.";
    private const string Field = "ReportDate";

    public CheckResult Check(object subject)
    {
        bool hasReportDate = reportDate != DateTimeOffset.MinValue;

        return hasReportDate
            ? CheckResult.Pass
            : CheckResult.Found(new Finding(Code, Message, Field));
    }
}
