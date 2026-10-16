using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.Claims.Rules;

internal sealed class HasClaimantForClosure : IRule<Claim>
{
    public const string Code = "S:CC-03";

    private const string Message = "Claim cannot be closed - at least one claimant is not satisfied.";
    private const string Field = "ClaimParties";

    public CheckResult Check(Claim claim)
    {
        bool hasClaimant = claim.HasActiveClaimant;

        return hasClaimant
            ? CheckResult.Pass
            : CheckResult.Found(new Finding(Code, Message, Field));
    }
}
