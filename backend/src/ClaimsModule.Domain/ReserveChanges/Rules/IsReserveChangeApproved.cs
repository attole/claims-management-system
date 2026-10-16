using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.ReserveChanges.Rules;

internal sealed class IsReserveChangeApproved : IRule<ReserveChange>
{
    public const string Code = "RESERVE_CHANGE_NOT_APPROVED";

    private const string Message = "Only an approved reserve change can be posted.";
    private const string Field = "ApprovalState";

    public CheckResult Check(ReserveChange change)
    {
        bool isApproved = change.ApprovalState.IsApproved;

        return isApproved
            ? CheckResult.Pass
            : CheckResult.Found(new Finding(Code, Message, Field));
    }
}
