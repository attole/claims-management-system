using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.ReserveComponents.History.Rules;

internal sealed class IsApprovedBalanceNonNegative(decimal approvedBalance) : IRule<ReserveHistory>
{
    public const string Code = "S:BR-R-01";

    private const string Message = "Reserve balance cannot go below zero.";
    private const string Field = "Amount";

    public CheckResult Check(ReserveHistory history)
    {
        bool isNonNegative = history.Details.CanHoldBalanceAfter(approvedBalance);

        return isNonNegative
            ? CheckResult.Pass
            : CheckResult.Found(new Finding(Code, Message, Field));
    }
}
