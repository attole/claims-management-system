using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.Claims.Parties.Rules;

internal sealed class IsPartyActive : IRule<ClaimParty>
{
    public const string Code = "PARTY_NOT_ACTIVE";

    private const string Message = "The party has been removed.";
    private const string Field = "PartyId";

    public CheckResult Check(ClaimParty party)
    {
        bool isPartyActive = party.IsActive;

        return isPartyActive
            ? CheckResult.Pass
            : CheckResult.Found(new Finding(Code, Message, Field));
    }
}
