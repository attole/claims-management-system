using ClaimsModule.Domain.Core.Rules;
using ClaimsModule.Domain.Reserves;

namespace ClaimsModule.Domain.ReserveChanges.Rules;

internal sealed class HasNonNegativeBalance(decimal balanceAfter, ReserveComponentType componentType)
    : IRule<object>
{
    public const string Code = "S:BR-R-01";

    private const string Message = "Reserve balance cannot go below zero.";
    private const string Field = "Amount";

    public CheckResult Check(object subject)
    {
        bool hasNonNegativeBalance = componentType.CanHoldBalance(balanceAfter);

        return hasNonNegativeBalance
            ? CheckResult.Pass
            : CheckResult.Found(new Finding(Code, Message, Field));
    }
}
