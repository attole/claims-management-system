using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.Claims.Loss.Rules;

internal sealed class IsDescriptionLongEnough : IRule<LossDetails>
{
    public const string Code = "S:BR-C-07";

    private const string Message = "Loss description is required and must be at least 20 characters.";
    private const string Field = "LossDescription";

    public CheckResult Check(LossDetails loss)
    {
        bool isLongEnough = loss.HasLongEnoughDescription;
        var finding = new Finding(Code, Message, Field) { Evidence = loss.TrimmedDescription };

        return isLongEnough
            ? CheckResult.Pass
            : CheckResult.Found(finding);
    }
}
