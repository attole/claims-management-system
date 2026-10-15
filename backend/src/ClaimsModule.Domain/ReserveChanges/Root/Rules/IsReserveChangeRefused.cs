using ClaimsModule.Domain.Core.Base.Checks;
using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.ReserveChanges.Root.Rules;

public sealed class IsReserveChangeRefused : IRule<ReserveChange>
{
    public const string Code = "RESERVE_CHANGE_NOT_REFUSED";

    private const string Message = "Only a rejected or retracted reserve change can have its posting cancelled.";
    private const string Field = "ApprovalState";

    public CheckResult<DomainFinding> Check(ReserveChange reserve)
    {
        bool isRefused = reserve.ApprovalState is ReserveRejectedState or ReserveCancelledState;

        return isRefused
            ? CheckResult<DomainFinding>.Pass
            : CheckResult<DomainFinding>.Found(new RuleFinding(Code, Message, Field));
    }
}
