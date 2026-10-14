using ClaimsModule.Domain.Claims.Loss;
using ClaimsModule.Domain.Claims.Loss.Operations;
using ClaimsModule.Domain.Claims.Parties;
using ClaimsModule.Domain.Claims.Parties.Operations;
using ClaimsModule.Domain.Claims.RiskObjects;
using ClaimsModule.Domain.Claims.RiskObjects.Operations;
using ClaimsModule.Domain.Claims.Root;
using ClaimsModule.Domain.Claims.Root.Operations;

namespace ClaimsModule.Domain.Tests.Support.Claims;

public static class TestClaims
{
    public static readonly DateTimeOffset Start = new(2026, 10, 12, 8, 0, 0, TimeSpan.Zero);
    public static readonly DateTimeOffset Later = Start.AddHours(1);
    public static readonly Guid Organisation = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid Actor = Guid.Parse("22222222-2222-2222-2222-222222222222");
    public const string Description = "Water damage in the kitchen after a burst pipe";

    public static Claim Empty()
    {
        var input = new CreateClaimInput(Organisation, ClaimNumber.From("CLM-2026-0000001"), Actor, Start);

        return new CreateClaimOperation().Execute(input).Value;
    }

    public static Claim Create(params PartyDetails[] parties)
    {
        var claim = Empty();
        var riskObject = new RiskObjectDetails(AssetType.Property, "Kitchen");

        SetLoss(claim, Loss(Start.AddDays(-3), Description));
        new AddRiskObjectOperation().Execute(claim, new AddRiskObjectInput(riskObject, Actor, Start));

        foreach (var party in parties)
        {
            new AddPartyOperation().Execute(claim, new AddPartyInput(party, Actor, Start));
        }

        claim.PopEvents();

        return claim;
    }

    public static Claim WithLoss(DateTimeOffset lossDate)
    {
        var claim = Create(Claimant());
        SetLoss(claim, Loss(lossDate, Description));
        claim.PopEvents();

        return claim;
    }

    public static Claim WithDescription(string description)
    {
        var claim = Create(Claimant());
        SetLoss(claim, Loss(Start.AddDays(-3), description));
        claim.PopEvents();

        return claim;
    }

    public static void SetLoss(Claim claim, LossDetails loss)
    {
        new UpdateLossDetailsOperation().Execute(claim, new UpdateLossDetailsInput(loss, Actor, Start));
    }

    public static LossDetails Loss(DateTimeOffset lossDate, string description)
    {
        return new LossDetails(new LossDate(lossDate), description, "WATER");
    }

    public static PartyDetails Claimant()
    {
        return new PartyDetails(new PartyClaimantRole(), new PartyPersonType())
        {
            FirstName = "Ada",
            LastName = "Stone",
        };
    }

    public static PartyDetails Witness()
    {
        return new PartyDetails(new PartyWitnessRole(), new PartyPersonType()) { FirstName = "Bo", LastName = "Reed" };
    }
}
