using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.ReserveChanges.Rules;

internal sealed class IsApprovedBalanceNonNegative(decimal approvedBalance) : IRule<ReserveChange>
{
    public const string Code = "S:BR-R-01";

    private const string Message = "Reserve balance cannot go below zero.";
    private const string Field = "Amount";

    public CheckResult Check(ReserveChange change)
    {
        bool isNonNegative = change.Details.CanHoldBalanceAfter(approvedBalance);

        return isNonNegative
            ? CheckResult.Pass
            : CheckResult.Found(new Finding(Code, Message, Field));
    }
}
