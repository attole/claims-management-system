using ClaimsModule.Domain.Core.Base.Checks;
using ClaimsModule.Domain.Core.Operations;
using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.ReserveChanges.Root.Rules;

public sealed class IsReserveAmountNonZero(decimal amount) : IRule<NoSubject>
{
    public const string Code = "RESERVE_AMOUNT_ZERO";

    private const string Message = "Reserve amount must not be zero.";
    private const string Field = "Amount";

    public CheckResult<DomainFinding> Check(NoSubject subject)
    {
        bool isNonZero = amount != 0m;

        return isNonZero
            ? CheckResult<DomainFinding>.Pass
            : CheckResult<DomainFinding>.Found(new RuleFinding(Code, Message, Field));
    }
}
