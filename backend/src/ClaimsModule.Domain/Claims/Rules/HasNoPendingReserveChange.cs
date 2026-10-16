using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.Claims.Rules;

internal sealed class HasNoPendingReserveChange(bool hasPendingReserveChange) : IRule<object>
{
    public const string Code = "S:CC-01";

    private const string Message = "Claim cannot be closed - approval of every pending reserve is not satisfied.";
    private const string Field = "Reserves";

    public CheckResult Check(object subject)
    {
        bool hasNoPendingReserveChange = !hasPendingReserveChange;

        return hasNoPendingReserveChange
            ? CheckResult.Pass
            : CheckResult.Found(new Finding(Code, Message, Field));
    }
}
