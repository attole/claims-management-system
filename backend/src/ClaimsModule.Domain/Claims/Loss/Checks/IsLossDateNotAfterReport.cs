using ClaimsModule.Domain.Core.Base.Checks;
using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.Claims.Loss.Checks;

internal sealed class IsLossDateNotAfterReport(DateTimeOffset reportDate) : ICheck<LossDetails, DomainFinding>
{
    public const string Code = "LOSS_DATE_AFTER_REPORT";

    private const string Message = "Loss date cannot be after the date the claim was reported.";
    private const string Field = "LossDate";

    public CheckResult<DomainFinding> Check(LossDetails loss)
    {
        bool isNotAfterReport = !loss.LossDate.IsSet || loss.LossDate.Value <= reportDate;

        return isNotAfterReport
            ? CheckResult<DomainFinding>.Pass
            : CheckResult<DomainFinding>.Found(new RuleFinding(Code, Message, Field));
    }
}
