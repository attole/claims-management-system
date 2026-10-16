using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.Claims.Rules;

internal sealed class HasApprovedReserveForClaimUnderInvestigation(bool hasApprovedReserve) : IRule<object>
{
    public const string Code = "S:ST-UnderInvestigation-PendingPayment";

    private const string Message = "An approved reserve is required to move the claim to pending payment.";
    private const string Field = "Reserves";

    public CheckResult Check(object subject)
    {
        bool hasApprovedReserveForClaimUnderInvestigation = hasApprovedReserve;

        return hasApprovedReserveForClaimUnderInvestigation
            ? CheckResult.Pass
            : CheckResult.Found(new Finding(Code, Message, Field));
    }
}
