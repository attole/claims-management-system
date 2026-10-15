using ClaimsModule.Domain.Claims.Root;
using ClaimsModule.Domain.Core.Base.Checks;

namespace ClaimsModule.Domain.Claims.Issues.Checks;

internal sealed class IsLossDatePresent : ICheck<Claim, IssueFinding>
{
    public const string Code = "S:V-LossDate-Required";

    private const string Message = "Loss date is required.";
    private const string Field = "LossDate";

    public CheckResult<IssueFinding> Check(Claim claim)
    {
        bool hasLossDate = claim.LossEvent.Details.LossDate.IsSet;

        return hasLossDate
            ? CheckResult<IssueFinding>.Pass
            : CheckResult<IssueFinding>.Found(IssueFinding.Critical(Code, Message, Field));
    }
}
