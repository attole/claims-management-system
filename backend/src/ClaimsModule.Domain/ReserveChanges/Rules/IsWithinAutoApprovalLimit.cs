using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.ReserveChanges.Rules;

internal sealed class IsWithinAutoApprovalLimit : IRule<ReserveChange>
{
    public const string Code = "S:BR-R-02";

    private const string Message = "This reserve needs an approver.";
    private const string Field = "Amount";

    public CheckResult Check(ReserveChange change)
    {
        bool isWithinLimit = change.Details.IsWithinAutoApprovalLimit;

        return isWithinLimit
            ? CheckResult.Pass
            : CheckResult.Found(new Finding(Code, Message, Field));
    }
}
