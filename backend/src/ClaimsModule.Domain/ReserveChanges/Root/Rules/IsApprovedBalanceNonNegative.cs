using ClaimsModule.Domain.Core.Base.Checks;
using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.ReserveChanges.Root.Rules;

public sealed class IsApprovedBalanceNonNegative(ReserveSnapshot snapshot) : IRule<ReserveChange>
{
    public const string Code = "S:BR-R-01";

    private const string Message = "Reserve balance cannot go below zero.";
    private const string Field = "Amount";

    public CheckResult<DomainFinding> Check(ReserveChange reserve)
    {
        decimal balanceAfter = snapshot.ReserveApprovedBalance + reserve.Details.Amount;
        bool isNonNegative = snapshot.ComponentType.CanGoNegative || balanceAfter >= 0m;

        return isNonNegative
            ? CheckResult<DomainFinding>.Pass
            : CheckResult<DomainFinding>.Found(new RuleFinding(Code, Message, Field));
    }
}
