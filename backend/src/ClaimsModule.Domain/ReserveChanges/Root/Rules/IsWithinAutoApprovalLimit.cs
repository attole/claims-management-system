using ClaimsModule.Domain.Core.Base.Checks;
using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.ReserveChanges.Root.Rules;

public sealed class IsWithinAutoApprovalLimit : IRule<ReserveChange>
{
    public const string Code = "S:BR-R-02";

    private const decimal AutoApprovalLimit = 10_000m;
    private const string Message = "This reserve needs an approver.";
    private const string Field = "Amount";

    public CheckResult<DomainFinding> Check(ReserveChange reserve)
    {
        bool isWithinLimit = Math.Abs(reserve.Details.Amount) <= AutoApprovalLimit;

        return isWithinLimit
            ? CheckResult<DomainFinding>.Pass
            : CheckResult<DomainFinding>.Found(new RuleFinding(Code, Message, Field));
    }
}
