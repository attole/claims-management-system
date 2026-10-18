using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.Claims.Rules;

internal sealed class HasNoPendingReserveHistory(bool hasPendingReserveHistory) : IRule<object>
{
    public const string Code = "S:CC-01";

    private const string Message = "Claim cannot be closed - approval of every pending reserve is not satisfied.";
    private const string Field = "Reserves";

    public CheckResult Check(object subject)
    {
        bool hasNoPendingReserveHistory = !hasPendingReserveHistory;

        return hasNoPendingReserveHistory
            ? CheckResult.Pass
            : CheckResult.Found(new Finding(Code, Message, Field));
    }
}
