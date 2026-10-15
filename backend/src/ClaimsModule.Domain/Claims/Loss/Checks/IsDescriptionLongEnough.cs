using ClaimsModule.Domain.Core.Base.Checks;
using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.Claims.Loss.Checks;

internal sealed class IsDescriptionLongEnough : ICheck<LossDetails, DomainFinding>
{
    public const string Code = "S:BR-C-07";
    public const int MinimumLength = 20;

    private const string Message = "Loss description is required and must be at least 20 characters.";
    private const string Field = "LossDescription";

    public CheckResult<DomainFinding> Check(LossDetails loss)
    {
        bool isLongEnough = loss.Description.Trim().Length >= MinimumLength;

        return isLongEnough
            ? CheckResult<DomainFinding>.Pass
            : CheckResult<DomainFinding>.Found(new RuleFinding(Code, Message, Field));
    }
}
