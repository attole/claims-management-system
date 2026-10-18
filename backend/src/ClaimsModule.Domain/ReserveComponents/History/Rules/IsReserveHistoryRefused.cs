using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.ReserveComponents.History.Rules;

internal sealed class IsReserveHistoryRefused : IRule<ReserveHistory>
{
    public const string Code = "RESERVE_HISTORY_NOT_REFUSED";

    private const string Message = "Only a rejected or retracted reserve change can have its posting cancelled.";
    private const string Field = "ApprovalState";

    public CheckResult Check(ReserveHistory history)
    {
        bool isRefused = history.ApprovalState.IsRefused;

        return isRefused
            ? CheckResult.Pass
            : CheckResult.Found(new Finding(Code, Message, Field));
    }
}
