using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.ReserveChanges.Rules;

internal sealed class IsReserveChangeRefused : IRule<ReserveChange>
{
    public const string Code = "RESERVE_CHANGE_NOT_REFUSED";

    private const string Message = "Only a rejected or retracted reserve change can have its posting cancelled.";
    private const string Field = "ApprovalState";

    public CheckResult Check(ReserveChange change)
    {
        bool isRefused = change.ApprovalState.IsRefused;

        return isRefused
            ? CheckResult.Pass
            : CheckResult.Found(new Finding(Code, Message, Field));
    }
}
