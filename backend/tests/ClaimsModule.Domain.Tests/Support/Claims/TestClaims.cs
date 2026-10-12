using ClaimsModule.Domain.Claims.Loss;
using ClaimsModule.Domain.Claims.Loss.Operations;
using ClaimsModule.Domain.Claims.Parties;
using ClaimsModule.Domain.Claims.Parties.Operations;
using ClaimsModule.Domain.Claims.RiskObjects;
using ClaimsModule.Domain.Claims.RiskObjects.Operations;
using ClaimsModule.Domain.Claims.Root;

namespace ClaimsModule.Domain.Tests.Support.Claims;

public static class TestClaims
{
    public static readonly DateTimeOffset Start = new(2026, 10, 12, 8, 0, 0, TimeSpan.Zero);
    public static readonly DateTimeOffset Later = Start.AddHours(1);
    public static readonly Guid Organisation = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid Actor = Guid.Parse("22222222-2222-2222-2222-222222222222");

    public static Claim Empty()
    {
        return Claim.Create(
            Organisation,
            ClaimNumber.From("CLM-2026-0000001"),
            Actor,
            Start);
    }

    public static Claim Create(params PartyDetails[] parties)
    {
        Claim claim = Empty();
        LossDetails loss = new(new LossDate(Start.AddDays(-3)), "Water damage", "WATER");
        RiskObjectDetails riskObject = new(AssetType.Property, "Kitchen");

        new UpdateLossDetailsOperation().Execute(claim, new UpdateLossDetailsInput(loss, Start));
        new AddRiskObjectOperation().Execute(claim, new AddRiskObjectInput(riskObject, Start));

        foreach (PartyDetails party in parties)
        {
            new AddPartyOperation().Execute(claim, new AddPartyInput(party, Actor, Start));
        }

        claim.PopEvents();

        return claim;
    }

    public static PartyDetails Claimant()
    {
        return new PartyDetails(new ClaimantRole(), new PersonPartyType()) { FirstName = "Ada", LastName = "Stone" };
    }

    public static PartyDetails Witness()
    {
        return new PartyDetails(new WitnessRole(), new PersonPartyType()) { FirstName = "Bo", LastName = "Reed" };
    }
}
