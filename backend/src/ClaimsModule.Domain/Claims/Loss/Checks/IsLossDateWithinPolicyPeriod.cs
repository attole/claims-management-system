using ClaimsModule.Domain.Claims.Root;
using ClaimsModule.Domain.Core.Base.Checks;
using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.Claims.Loss.Checks;

internal sealed class IsLossDateWithinPolicyPeriod(PolicyPeriod period) : ICheck<LossDetails, DomainFinding>
{
    public const string Code = "S:BR-C-02";

    private const string Message = "Loss date is outside the policy effective period.";
    private const string Field = "LossDate";

    public CheckResult<DomainFinding> Check(LossDetails loss)
    {
        var lossDay = DateOnly.FromDateTime(loss.LossDate.Value.UtcDateTime);
        bool isWithinPeriod = !loss.LossDate.IsSet || !period.IsSet || period.CanCover(lossDay);

        return isWithinPeriod
            ? CheckResult<DomainFinding>.Pass
            : CheckResult<DomainFinding>.Found(new RuleFinding(Code, Message, Field).AsWarning());
    }
}
