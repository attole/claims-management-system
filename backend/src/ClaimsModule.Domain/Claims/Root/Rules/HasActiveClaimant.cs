using ClaimsModule.Domain.Core.Base.Checks;
using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.Claims.Root.Rules;

public sealed class HasActiveClaimant : IRule<Claim>
{
    public const string Code = "S:BR-C-03";

    private const string Message = "At least one Claimant party is required to open a claim.";
    private const string Field = "ClaimParties";

    public CheckResult<DomainFinding> Check(Claim claim)
    {
        bool hasActiveClaimant = claim.ActiveClaimantCount > 0;

        return hasActiveClaimant
            ? CheckResult<DomainFinding>.Pass
            : CheckResult<DomainFinding>.Found(new RuleFinding(Code, Message, Field));
    }
}
