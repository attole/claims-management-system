using ClaimsModule.Domain.Common.Roles;
using ClaimsModule.Domain.Core.Base.Checks;
using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.ReserveChanges.Root.Rules;

internal sealed class HasApprovalRole(UserRole actorRole) : IRule<ReserveChange>
{
    public const string Code = "RESERVE_APPROVER_ROLE_REQUIRED";

    private const string Message = "Only a supervisor or a manager can decide on a reserve.";
    private const string Field = "ActorRole";

    public CheckResult<DomainFinding> Check(ReserveChange reserve)
    {
        bool hasApprovalRole = actorRole is UserRole.Supervisor or UserRole.Manager;

        return hasApprovalRole
            ? CheckResult<DomainFinding>.Pass
            : CheckResult<DomainFinding>.Found(new RuleFinding(Code, Message, Field));
    }
}
