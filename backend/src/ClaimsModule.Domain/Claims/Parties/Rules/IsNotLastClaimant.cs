using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.Claims.Parties.Rules;

internal sealed class IsNotLastClaimant(Guid partyId) : IRule<Claim>
{
    public const string Code = "S:PARTY-LastClaimant";

    private const string Message = "The last claimant cannot be removed.";
    private const string Field = "PartyId";

    public CheckResult Check(Claim claim)
    {
        bool isNotLastClaimant = !claim.IsOnlyActiveClaimant(partyId);

        return isNotLastClaimant
            ? CheckResult.Pass
            : CheckResult.Found(new Finding(Code, Message, Field));
    }
}
