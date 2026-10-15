using ClaimsModule.Domain.Core.Base.Checks;
using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.Claims.Loss.Checks;

internal sealed class IsLossDateNotInFuture(DateTimeOffset now) : ICheck<LossDetails, DomainFinding>
{
    public const string Code = "S:BR-C-01";

    private const string Message = "Loss date cannot be in the future.";
    private const string Field = "LossDate";

    public CheckResult<DomainFinding> Check(LossDetails loss)
    {
        bool isNotInFuture = !loss.LossDate.IsSet || loss.LossDate.Value <= now;

        return isNotInFuture
            ? CheckResult<DomainFinding>.Pass
            : CheckResult<DomainFinding>.Found(new RuleFinding(Code, Message, Field));
    }
}
