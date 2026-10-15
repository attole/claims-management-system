using ClaimsModule.Domain.Claims.Root;
using ClaimsModule.Domain.Core.Base.Checks;

namespace ClaimsModule.Domain.Claims.Issues.Checks;

public sealed class IsLossDateWithinPolicyPeriod(PolicyPeriod period) : ICheck<Claim, IssueFinding>
{
    public const string Code = "S:BR-C-02";

    private const string Message = "Loss date is outside the policy effective period.";
    private const string Field = "LossDate";

    public CheckResult<IssueFinding> Check(Claim claim)
    {
        var lossDate = claim.LossEvent.Details.LossDate;
        var lossDay = DateOnly.FromDateTime(lossDate.Value.UtcDateTime);
        bool isWithinPeriod = !lossDate.IsSet || !period.IsSet || period.CanCover(lossDay);

        return isWithinPeriod
            ? CheckResult<IssueFinding>.Pass
            : CheckResult<IssueFinding>.Found(IssueFinding.Warning(Code, Message, Field));
    }
}
