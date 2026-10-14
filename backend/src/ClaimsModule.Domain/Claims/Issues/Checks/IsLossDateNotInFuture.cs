using ClaimsModule.Domain.Claims.Root;
using ClaimsModule.Domain.Core.Base.Checks;

namespace ClaimsModule.Domain.Claims.Issues.Checks;

public sealed class IsLossDateNotInFuture(DateTimeOffset now) : ICheck<Claim, IssueFinding>
{
    public const string Code = "S:BR-C-01";

    private const string Message = "Loss date cannot be in the future.";
    private const string Field = "LossDate";

    public CheckResult<IssueFinding> Check(Claim claim)
    {
        var lossDate = claim.LossEvent.Details.LossDate;
        bool isNotInFuture = !lossDate.IsSet || lossDate.Value <= now;

        return isNotInFuture
            ? CheckResult<IssueFinding>.Pass
            : CheckResult<IssueFinding>.Found(IssueFinding.Critical(Code, Message, Field));
    }
}
