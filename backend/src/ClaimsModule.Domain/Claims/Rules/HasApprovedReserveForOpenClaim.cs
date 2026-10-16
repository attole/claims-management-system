using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.Claims.Rules;

internal sealed class HasApprovedReserveForOpenClaim(bool hasApprovedReserve) : IRule<object>
{
    public const string Code = "S:ST-Open-PendingPayment";

    private const string Message = "An approved reserve is required to move the claim to pending payment.";
    private const string Field = "Reserves";

    public CheckResult Check(object subject)
    {
        bool hasApprovedReserveForOpenClaim = hasApprovedReserve;

        return hasApprovedReserveForOpenClaim
            ? CheckResult.Pass
            : CheckResult.Found(new Finding(Code, Message, Field));
    }
}
