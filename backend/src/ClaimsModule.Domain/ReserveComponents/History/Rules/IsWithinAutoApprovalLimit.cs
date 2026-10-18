using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.ReserveComponents.History.Rules;

internal sealed class IsWithinAutoApprovalLimit : IRule<ReserveHistory>
{
    public const string Code = "S:BR-R-02";

    private const string Message = "This reserve needs an approver.";
    private const string Field = "Amount";

    public CheckResult Check(ReserveHistory history)
    {
        bool isWithinLimit = history.Details.IsWithinAutoApprovalLimit;

        return isWithinLimit
            ? CheckResult.Pass
            : CheckResult.Found(new Finding(Code, Message, Field));
    }
}
