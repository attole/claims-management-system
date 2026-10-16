using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.Claims.Loss.Rules;

internal sealed class IsLossDatePresent : IRule<LossDetails>
{
    public const string Code = "S:V-LossDate-Required";

    private const string Message = "Loss date is required.";
    private const string Field = "LossDate";

    public CheckResult Check(LossDetails loss)
    {
        bool hasLossDate = loss.LossDate.IsSet;

        return hasLossDate
            ? CheckResult.Pass
            : CheckResult.Found(new Finding(Code, Message, Field));
    }
}
