using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.Claims.Parties.Rules;

internal sealed class HasClaimant : IRule<Claim>
{
    public const string Code = "S:BR-C-03";

    private const string Message = "At least one Claimant party is required to open a claim.";
    private const string Field = "ClaimParties";

    public CheckResult Check(Claim claim)
    {
        bool hasClaimant = claim.HasActiveClaimant;

        return hasClaimant
            ? CheckResult.Pass
            : CheckResult.Found(new Finding(Code, Message, Field));
    }
}
