using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.ReserveComponents.History.Rules;

internal sealed class IsReserveHistoryApproved : IRule<ReserveHistory>
{
    public const string Code = "RESERVE_HISTORY_NOT_APPROVED";

    private const string Message = "Only an approved reserve change can be posted.";
    private const string Field = "ApprovalState";

    public CheckResult Check(ReserveHistory history)
    {
        bool isApproved = history.ApprovalState.IsApproved;

        return isApproved
            ? CheckResult.Pass
            : CheckResult.Found(new Finding(Code, Message, Field));
    }
}
