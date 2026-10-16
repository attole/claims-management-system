using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.Claims.Loss.Rules;

internal sealed class IsCauseCodeActive(bool isActiveCauseCode) : IRule<LossDetails>
{
    public const string Code = "S:BR-C-05";

    private const string Message = "Cause of loss code is not recognised or is inactive.";
    private const string Field = "CauseOfLossCode";

    public CheckResult Check(LossDetails loss)
    {
        var finding = new Finding(Code, Message, Field) { Evidence = loss.CauseOfLossCode };

        return isActiveCauseCode
            ? CheckResult.Pass
            : CheckResult.Found(finding);
    }
}
