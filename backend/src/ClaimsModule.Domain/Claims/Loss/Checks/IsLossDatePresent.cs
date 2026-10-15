using ClaimsModule.Domain.Core.Base.Checks;
using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.Claims.Loss.Checks;

internal sealed class IsLossDatePresent : ICheck<LossDetails, DomainFinding>
{
    public const string Code = "S:V-LossDate-Required";

    private const string Message = "Loss date is required.";
    private const string Field = "LossDate";

    public CheckResult<DomainFinding> Check(LossDetails loss)
    {
        bool hasLossDate = loss.LossDate.IsSet;

        return hasLossDate
            ? CheckResult<DomainFinding>.Pass
            : CheckResult<DomainFinding>.Found(new RuleFinding(Code, Message, Field));
    }
}
