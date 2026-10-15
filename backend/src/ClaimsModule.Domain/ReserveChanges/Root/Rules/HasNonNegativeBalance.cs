using ClaimsModule.Domain.Core.Base.Checks;
using ClaimsModule.Domain.Core.Operations;
using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.ReserveChanges.Root.Rules;

internal sealed class HasNonNegativeBalance(ReserveSnapshot snapshot, decimal amount) : IRule<NoSubject>
{
    public const string Code = "S:BR-R-01";

    private const string Message = "Reserve balance cannot go below zero.";
    private const string Field = "Amount";

    public CheckResult<DomainFinding> Check(NoSubject subject)
    {
        decimal balanceAfter = snapshot.ReserveApprovedBalance + amount;
        bool hasNonNegativeBalance = snapshot.ComponentType.CanGoNegative || balanceAfter >= 0m;

        return hasNonNegativeBalance
            ? CheckResult<DomainFinding>.Pass
            : CheckResult<DomainFinding>.Found(new RuleFinding(Code, Message, Field));
    }
}
