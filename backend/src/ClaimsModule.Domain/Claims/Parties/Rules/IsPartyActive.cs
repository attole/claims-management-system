using ClaimsModule.Domain.Claims.Root;
using ClaimsModule.Domain.Core.Base.Checks;
using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.Claims.Parties.Rules;

public sealed class IsPartyActive(Guid partyId) : IRule<Claim>
{
    public const string Code = "PARTY_NOT_ACTIVE";

    private const string Message = "The party is not on the claim or has been removed.";
    private const string Field = "PartyId";

    public CheckResult<DomainFinding> Check(Claim claim)
    {
        bool isPartyActive = claim.Parties.Any(party => party.Id == partyId && party.IsActive);

        return isPartyActive
            ? CheckResult<DomainFinding>.Pass
            : CheckResult<DomainFinding>.Found(new RuleFinding(Code, Message, Field));
    }
}
