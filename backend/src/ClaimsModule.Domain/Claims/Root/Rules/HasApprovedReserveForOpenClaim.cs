using ClaimsModule.Domain.Core.Base.Checks;
using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.Claims.Root.Rules;

public sealed class HasApprovedReserveForOpenClaim(ClaimReserveSnapshot reserves) : IRule<Claim>
{
    public const string Code = "S:ST-Open-PendingPayment";

    private const string Message = "An approved reserve is required to move the claim to pending payment.";
    private const string Field = "Reserves";

    public CheckResult<DomainFinding> Check(Claim claim)
    {
        bool hasApprovedReserve = reserves.ApprovedReserveCount > 0;

        return hasApprovedReserve
            ? CheckResult<DomainFinding>.Pass
            : CheckResult<DomainFinding>.Found(new RuleFinding(Code, Message, Field));
    }
}
