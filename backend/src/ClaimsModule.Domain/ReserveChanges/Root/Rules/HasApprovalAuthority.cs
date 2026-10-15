using ClaimsModule.Domain.Common.Roles;
using ClaimsModule.Domain.Core.Base.Checks;
using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.ReserveChanges.Root.Rules;

internal sealed class HasApprovalAuthority(UserRole actorRole) : IRule<ReserveChange>
{
    public const string Code = "S:BR-R-02";

    private const decimal SupervisorLimit = 100_000m;
    private const string Message = "Your role does not have authority to approve this reserve amount.";
    private const string Field = "ActorRole";

    public CheckResult<DomainFinding> Check(ReserveChange reserve)
    {
        decimal amount = Math.Abs(reserve.Details.Amount);
        bool hasAuthority = actorRole switch
        {
            UserRole.Manager => true,
            UserRole.Supervisor => amount <= SupervisorLimit,
            _ => false,
        };

        return hasAuthority
            ? CheckResult<DomainFinding>.Pass
            : CheckResult<DomainFinding>.Found(new RuleFinding(Code, Message, Field));
    }
}
