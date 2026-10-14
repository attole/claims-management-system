using ClaimsModule.Domain.Claims.Root;
using ClaimsModule.Domain.Core.Base.Checks;
using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.Claims.Parties.Rules;

public sealed class IsNotLastClaimant(Guid partyId) : IRule<Claim>
{
    public const string Code = "S:PARTY-LastClaimant";

    private const string Message = "The last claimant cannot be removed.";
    private const string Field = "PartyId";

    public CheckResult<DomainFinding> Check(Claim claim)
    {
        bool isNotLastClaimant = !claim.Parties
            .Where(party => party.IsActiveClaimant)
            .Select(party => party.Id)
            .SequenceEqual([partyId]);

        return isNotLastClaimant
            ? CheckResult<DomainFinding>.Pass
            : CheckResult<DomainFinding>.Found(new RuleFinding(Code, Message, Field));
    }
}
