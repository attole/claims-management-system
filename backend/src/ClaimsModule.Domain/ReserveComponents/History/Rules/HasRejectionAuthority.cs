using ClaimsModule.Domain.Core.Rules;
using ClaimsModule.Domain.Roles;

namespace ClaimsModule.Domain.ReserveComponents.History.Rules;

internal sealed class HasRejectionAuthority(UserRole actorRole) : IRule<ReserveHistory>
{
    public const string Code = "S:BR-R-02";

    private const string Message = "Your role does not have authority to reject this reserve amount.";
    private const string Field = "ActorRole";

    public CheckResult Check(ReserveHistory history)
    {
        bool hasAuthority = actorRole.CanDecideReserve(history.Details.AbsoluteAmount);

        return hasAuthority
            ? CheckResult.Pass
            : CheckResult.Found(new Finding(Code, Message, Field));
    }
}
