using ClaimsModule.Domain.Core.Base.Checks;
using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.Claims.Root.Rules;

internal sealed class HasClaimantForClosure : IRule<Claim>
{
    public const string Code = "S:CC-03";

    private const string Message = "Claim cannot be closed - at least one claimant is not satisfied.";
    private const string Field = "ClaimParties";

    public CheckResult<DomainFinding> Check(Claim claim)
    {
        bool hasClaimant = claim.ActiveClaimantCount > 0;

        return hasClaimant
            ? CheckResult<DomainFinding>.Pass
            : CheckResult<DomainFinding>.Found(new RuleFinding(Code, Message, Field));
    }
}
