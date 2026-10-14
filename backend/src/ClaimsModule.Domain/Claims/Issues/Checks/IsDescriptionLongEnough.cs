using ClaimsModule.Domain.Claims.Root;
using ClaimsModule.Domain.Core.Base.Checks;

namespace ClaimsModule.Domain.Claims.Issues.Checks;

public sealed class IsDescriptionLongEnough : ICheck<Claim, IssueFinding>
{
    public const string Code = "S:BR-C-07";
    public const int MinimumLength = 20;

    private const string Message = "Loss description is required and must be at least 20 characters.";
    private const string Field = "LossDescription";

    public CheckResult<IssueFinding> Check(Claim claim)
    {
        bool isLongEnough = claim.LossEvent.Details.Description.Trim().Length >= MinimumLength;

        return isLongEnough
            ? CheckResult<IssueFinding>.Pass
            : CheckResult<IssueFinding>.Found(IssueFinding.Critical(Code, Message, Field));
    }
}
