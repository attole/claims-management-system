using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.Claims.Rules;

internal sealed class HasNoPendingReserveHistoryForWithdrawal(bool hasPendingReserveHistory) : IRule<object>
{
    public const string Code = "CLAIM_WITHDRAW_PENDING_RESERVE";

    private const string Message = "Claim cannot be withdrawn while a reserve change awaits approval.";
    private const string Field = "Reserves";

    public CheckResult Check(object subject)
    {
        bool hasNoPendingReserveHistory = !hasPendingReserveHistory;

        return hasNoPendingReserveHistory
            ? CheckResult.Pass
            : CheckResult.Found(new Finding(Code, Message, Field));
    }
}
