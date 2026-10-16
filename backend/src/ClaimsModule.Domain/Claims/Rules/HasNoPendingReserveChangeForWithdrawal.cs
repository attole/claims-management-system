using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.Claims.Rules;

internal sealed class HasNoPendingReserveChangeForWithdrawal(bool hasPendingReserveChange) : IRule<object>
{
    public const string Code = "CLAIM_WITHDRAW_PENDING_RESERVE";

    private const string Message = "Claim cannot be withdrawn while a reserve change awaits approval.";
    private const string Field = "Reserves";

    public CheckResult Check(object subject)
    {
        bool hasNoPendingReserveChange = !hasPendingReserveChange;

        return hasNoPendingReserveChange
            ? CheckResult.Pass
            : CheckResult.Found(new Finding(Code, Message, Field));
    }
}
