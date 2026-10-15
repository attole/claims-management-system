using ClaimsModule.Domain.Core.Base.Checks;
using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.Claims.Root.Rules;

internal sealed class HasApprovedReserveForClaimUnderInvestigation(ClaimReserveSnapshot reserves) : IRule<Claim>
{
    public const string Code = "S:ST-UnderInvestigation-PendingPayment";

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
