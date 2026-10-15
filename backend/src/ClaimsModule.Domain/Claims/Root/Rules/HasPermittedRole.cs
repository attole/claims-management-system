using ClaimsModule.Domain.Common.Roles;
using ClaimsModule.Domain.Core.Base.Checks;
using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.Claims.Root.Rules;

internal sealed class HasPermittedRole(UserRole actorRole, params UserRole[] permittedRoles) : IRule<Claim>
{
    public const string Code = "ACTOR_ROLE_NOT_PERMITTED";

    private const string Message = "Your role is not permitted to make this change.";
    private const string Field = "ActorRole";

    public CheckResult<DomainFinding> Check(Claim claim)
    {
        bool hasPermittedRole = permittedRoles.Contains(actorRole);

        return hasPermittedRole
            ? CheckResult<DomainFinding>.Pass
            : CheckResult<DomainFinding>.Found(new RuleFinding(Code, Message, Field));
    }
}
