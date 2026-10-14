using ClaimsModule.Domain.Core.Base.Checks;
using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.Claims.Root.Rules;

public sealed class IsClaimNotWithdrawn : IRule<Claim>
{
    public const string Code = "CLAIM_WITHDRAWN";

    private const string Message = "A withdrawn claim cannot be edited.";
    private const string Field = "Status";

    public CheckResult<DomainFinding> Check(Claim claim)
    {
        bool isNotWithdrawn = claim.State.Marker is not ClaimStateKind.Withdrawn;

        return isNotWithdrawn
            ? CheckResult<DomainFinding>.Pass
            : CheckResult<DomainFinding>.Found(new RuleFinding(Code, Message, Field));
    }
}
