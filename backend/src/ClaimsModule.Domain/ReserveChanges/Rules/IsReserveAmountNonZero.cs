using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.ReserveChanges.Rules;

internal sealed class IsReserveAmountNonZero(decimal amount) : IRule<object>
{
    public const string Code = "RESERVE_AMOUNT_ZERO";

    private const string Message = "Reserve amount must not be zero.";
    private const string Field = "Amount";

    public CheckResult Check(object subject)
    {
        bool isNonZero = amount != 0m;

        return isNonZero
            ? CheckResult.Pass
            : CheckResult.Found(new Finding(Code, Message, Field));
    }
}
