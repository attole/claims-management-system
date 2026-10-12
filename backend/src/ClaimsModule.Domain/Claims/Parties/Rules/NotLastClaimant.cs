using ClaimsModule.Domain.Claims.Root;
using ClaimsModule.Domain.Common;
using ClaimsModule.Domain.Engine.Rules;

namespace ClaimsModule.Domain.Claims.Parties.Rules;

public sealed class NotLastClaimant(Guid partyId) : IRule<Claim>
{
    public const string Code = "S:PARTY-LastClaimant";

    public RuleResult Check(Claim claim)
    {
        bool isLastClaimant = claim.Parties
            .Where(party => party.IsActiveClaimant)
            .Select(party => party.Id)
            .SequenceEqual([partyId]);

        return isLastClaimant
            ? RuleResult.Fail(new RuleViolation(Code, "The last claimant cannot be removed."))
            : RuleResult.Pass;
    }
}
