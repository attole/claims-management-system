using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.ReserveComponents.History.Rules;

internal sealed class HasNonNegativeBalance(decimal amount) : IRule<ReserveComponent>
{
    public const string Code = "S:BR-R-01";

    private const string Message = "Reserve balance cannot go below zero.";
    private const string Field = "Amount";

    public CheckResult Check(ReserveComponent component)
    {
        bool hasNonNegativeBalance = component.CanHoldBalanceAfter(amount);

        return hasNonNegativeBalance
            ? CheckResult.Pass
            : CheckResult.Found(new Finding(Code, Message, Field));
    }
}
