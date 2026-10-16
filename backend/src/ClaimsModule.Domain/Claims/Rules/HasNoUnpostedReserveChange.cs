using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.Claims.Rules;

internal sealed class HasNoUnpostedReserveChange(bool hasUnpostedReserveChange) : IRule<object>
{
    public const string Code = "RESERVE_CHANGE_NOT_POSTED";

    private const string Message = "An approved reserve change is not posted yet.";
    private const string Field = "Reserves";

    public CheckResult Check(object subject)
    {
        bool hasNoUnpostedReserveChange = !hasUnpostedReserveChange;

        return hasNoUnpostedReserveChange
            ? CheckResult.Pass
            : CheckResult.Found(new Finding(Code, Message, Field));
    }
}
