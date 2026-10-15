using ClaimsModule.Domain.Claims.Root;
using ClaimsModule.Domain.Core.Base.Checks;

namespace ClaimsModule.Domain.Claims.Issues.Checks;

internal sealed class HasClaimant : ICheck<Claim, IssueFinding>
{
    public const string Code = "S:BR-C-03";

    private const string Message = "At least one Claimant party is required to open a claim.";
    private const string Field = "ClaimParties";

    public CheckResult<IssueFinding> Check(Claim claim)
    {
        bool hasClaimant = claim.ActiveClaimantCount > 0;

        return hasClaimant
            ? CheckResult<IssueFinding>.Pass
            : CheckResult<IssueFinding>.Found(IssueFinding.Critical(Code, Message, Field));
    }
}
