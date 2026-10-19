using ClaimsModule.Domain.CauseOfLoss;
using ClaimsModule.Domain.Claims;
using ClaimsModule.Domain.Claims.Inputs;
using ClaimsModule.Domain.Claims.Loss;
using ClaimsModule.Domain.Claims.Loss.Inputs;
using ClaimsModule.Domain.Claims.Parties;
using ClaimsModule.Domain.Claims.Parties.Inputs;
using ClaimsModule.Domain.Claims.RiskObjects;
using ClaimsModule.Domain.Claims.RiskObjects.Inputs;
using ClaimsModule.Domain.Claims.StateChanges;
using ClaimsModule.Domain.Core.Flows;
using ClaimsModule.Domain.Policies;

namespace ClaimsModule.Application.Tests.Support.Claims;

public static class ApplicationClaims
{
    private static readonly DateTimeOffset _start = new(2026, 10, 12, 8, 0, 0, TimeSpan.Zero);
    private static readonly Guid _organisation = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid _handler = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly Guid _policy = Guid.Parse("77777777-7777-7777-7777-777777777777");
    private static readonly PolicyPeriod _period = new(
        _policy,
        new DateOnly(2026, 1, 1),
        new DateOnly(2026, 12, 31),
        DateOnly.MinValue);
    private static readonly CauseCodes _codes = new(["WATER"]);

    public static Outcome<Claim> Created()
    {
        var input = new CreateClaimInput(
            _organisation,
            ClaimNumber.From("CLM-2026-0000001"),
            _start,
            _start,
            PolicyPeriod.Empty,
            _codes);

        return Claim.Create(input);
    }

    public static Outcome<Move<ClaimState>> OpenedWithoutDetails()
    {
        var claim = Created().Value;
        var input = new ClaimTransitionInput(PolicyPeriod.Empty, _codes);

        return claim.Open(input);
    }

    public static Outcome<Move<ClaimState>> WithdrawnWithOpenBalance()
    {
        var claim = Open();
        var input = new FreezeClaimInput(false, false, true, _period, _codes)
        {
            Reason = new StateChangeReason("WITHDRAWN", "Reported in error"),
            Note = "Open balance reviewed",
        };

        return claim.Withdraw(input);
    }

    public static Claim Draft()
    {
        return Created().Value;
    }

    public static Claim WithParties()
    {
        var claim = Draft();
        var claimant = new PartyDetails(new PartyClaimantRole(), new PartyPersonType())
        {
            FirstName = "Ada",
            LastName = "Stone",
        };
        var witness = new PartyDetails(new PartyWitnessRole(), new PartyPersonType())
        {
            FirstName = "Bo",
            LastName = "Reed",
        };

        claim.AddParty(new AddPartyInput(claimant, PolicyPeriod.Empty, _codes));
        claim.AddParty(new AddPartyInput(witness, PolicyPeriod.Empty, _codes));

        return claim;
    }

    public static Claim WithRiskObjects()
    {
        var claim = Draft();
        var kitchen = new RiskObjectDetails(AssetType.Property, "Kitchen");
        var garage = new RiskObjectDetails(AssetType.Property, "Garage");

        claim.AddRiskObject(new AddRiskObjectInput(kitchen, PolicyPeriod.Empty, _codes));
        claim.AddRiskObject(new AddRiskObjectInput(garage, PolicyPeriod.Empty, _codes));

        return claim;
    }

    private static Claim Open()
    {
        var claim = Created().Value;
        var loss = new LossDetails(new LossDate(_start.AddDays(-3)), "Water damage in the kitchen", "WATER");
        var person = new PartyDetails(new PartyClaimantRole(), new PartyPersonType())
        {
            FirstName = "Ada",
            LastName = "Stone",
        };
        var riskObject = new RiskObjectDetails(AssetType.Property, "Kitchen");

        claim.LinkPolicy(new LinkPolicyInput(_policy, _period, _codes));
        claim.UpdateLossDetails(new UpdateLossDetailsInput(loss, _period, _codes));
        claim.AddRiskObject(new AddRiskObjectInput(riskObject, _period, _codes));
        claim.AssignHandler(new AssignHandlerInput(_handler, _period, _codes));
        claim.AddParty(new AddPartyInput(person, _period, _codes));
        claim.Open(new ClaimTransitionInput(_period, _codes));

        return claim;
    }
}
