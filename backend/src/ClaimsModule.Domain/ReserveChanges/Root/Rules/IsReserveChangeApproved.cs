using ClaimsModule.Domain.Core.Base.Checks;
using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.ReserveChanges.Root.Rules;

public sealed class IsReserveChangeApproved : IRule<ReserveChange>
{
    public const string Code = "RESERVE_CHANGE_NOT_APPROVED";

    private const string Message = "Only an approved reserve change can be posted.";
    private const string Field = "ApprovalState";

    public CheckResult<DomainFinding> Check(ReserveChange reserve)
    {
        bool isApproved = reserve.IsApproved;

        return isApproved
            ? CheckResult<DomainFinding>.Pass
            : CheckResult<DomainFinding>.Found(new RuleFinding(Code, Message, Field));
    }
}
