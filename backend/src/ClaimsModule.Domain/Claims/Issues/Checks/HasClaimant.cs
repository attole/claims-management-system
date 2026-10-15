using ClaimsModule.Domain.Claims.Root;
using ClaimsModule.Domain.Core.Base.Checks;
using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.Claims.Issues.Checks;

internal sealed class HasClaimant : ICheck<Claim, DomainFinding>
{
    public const string Code = "S:BR-C-03";

    private const string Message = "At least one Claimant party is required to open a claim.";
    private const string Field = "ClaimParties";

    public CheckResult<DomainFinding> Check(Claim claim)
    {
        bool hasClaimant = claim.ActiveClaimantCount > 0;

        return hasClaimant
            ? CheckResult<DomainFinding>.Pass
            : CheckResult<DomainFinding>.Found(new RuleFinding(Code, Message, Field));
    }
}
