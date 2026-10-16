using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.Roles.Rules;

internal sealed class HasMinimumRole(UserRole actorRole, UserRole minimumRole) : IRule<object>
{
    public const string Code = "ACTOR_ROLE_NOT_PERMITTED";

    private const string Message = "Your role is not permitted to make this change.";
    private const string Field = "ActorRole";

    public CheckResult Check(object subject)
    {
        bool hasMinimumRole = actorRole.HasRightsOf(minimumRole);

        return hasMinimumRole
            ? CheckResult.Pass
            : CheckResult.Found(new Finding(Code, Message, Field));
    }
}
