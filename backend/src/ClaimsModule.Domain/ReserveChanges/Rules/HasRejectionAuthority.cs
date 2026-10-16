using ClaimsModule.Domain.Core.Rules;
using ClaimsModule.Domain.Roles;

namespace ClaimsModule.Domain.ReserveChanges.Rules;

internal sealed class HasRejectionAuthority(UserRole actorRole) : IRule<ReserveChange>
{
    public const string Code = "S:BR-R-02";

    private const string Message = "Your role does not have authority to reject this reserve amount.";
    private const string Field = "ActorRole";

    public CheckResult Check(ReserveChange change)
    {
        bool hasAuthority = actorRole.CanDecideReserve(change.Details.AbsoluteAmount);

        return hasAuthority
            ? CheckResult.Pass
            : CheckResult.Found(new Finding(Code, Message, Field));
    }
}
