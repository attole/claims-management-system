using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.Claims.Rules;

internal sealed class HasNoUnpostedReserveHistory(bool hasUnpostedReserveHistory) : IRule<object>
{
    public const string Code = "RESERVE_HISTORY_NOT_POSTED";

    private const string Message = "An approved reserve change is not posted yet.";
    private const string Field = "Reserves";

    public CheckResult Check(object subject)
    {
        bool hasNoUnpostedReserveHistory = !hasUnpostedReserveHistory;

        return hasNoUnpostedReserveHistory
            ? CheckResult.Pass
            : CheckResult.Found(new Finding(Code, Message, Field));
    }
}
