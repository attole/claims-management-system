using ClaimsModule.Domain.CauseOfLoss;
using ClaimsModule.Domain.Claims;
using ClaimsModule.Domain.Claims.Inputs;
using ClaimsModule.Domain.Claims.Loss;
using ClaimsModule.Domain.Claims.Loss.Inputs;
using ClaimsModule.Domain.Claims.Parties;
using ClaimsModule.Domain.Claims.Parties.Inputs;
using ClaimsModule.Domain.Claims.RiskObjects;
using ClaimsModule.Domain.Claims.RiskObjects.Inputs;
using ClaimsModule.Domain.Common;
using ClaimsModule.Domain.Policies;

namespace ClaimsModule.Persistence.Tests.Support.Claims;

internal static class TestClaims
{
    public const string CauseCode = "WATER";

    public const decimal EstimatedAmount = 2_500.5m;

    private static readonly CauseCodes _codes = new([CauseCode]);

    public static Claim Create(TestDatabase database, int sequence = 1)
    {
        var input = new CreateClaimInput(
            TestDatabase.OrganisationId,
            ClaimNumber.Create(2026, sequence),
            database.Now,
            database.Now,
            PolicyPeriod.Empty,
            _codes);

        return Claim.Create(input).Value;
    }

    public static Claim Ready(TestDatabase database, Guid policyId, int sequence = 1)
    {
        var claim = Create(database, sequence);
        var period = PeriodOf(policyId);
        var now = database.Now;
        var loss = new LossDetails(new LossDate(now.AddDays(-3)), "Water damage in the kitchen", CauseCode)
        {
            Location = "12 Main Street",
            EstimatedLossAmount = new EstimatedLossAmount(EstimatedAmount),
            PoliceReportNumber = "PR-1",
        };
        var claimant = new PartyDetails(new PartyClaimantRole(), new PartyPersonType())
        {
            FirstName = "Ada",
            LastName = "Stone",
            Email = "ada@example.com",
            Phone = "555-0100",
            Notes = "Prefers email",
        };
        var asset = new RiskObjectDetails(AssetType.Vehicle, "Delivery van")
        {
            DamageDescription = "Water in the engine bay",
            IsPrimary = true,
            AssetReference = "VIN-1",
        };

        claim.LinkPolicy(new LinkPolicyInput(policyId, period, _codes, now));
        claim.UpdateLossDetails(new UpdateLossDetailsInput(loss, period, _codes, now));
        claim.AddRiskObject(new AddRiskObjectInput(asset, period, _codes, now));
        claim.AssignHandler(new AssignHandlerInput(TestDatabase.UserId, period, _codes, now));
        claim.AddParty(new AddPartyInput(claimant, period, _codes, now));

        return claim;
    }

    public static Claim Opened(TestDatabase database, int sequence = 1)
    {
        var policyId = Ids.Next();
        var claim = Ready(database, policyId, sequence);

        claim.Open(new ClaimTransitionInput(PeriodOf(policyId), _codes, database.Now));

        return claim;
    }

    public static Guid Seed(TestDatabase database, int sequence = 1)
    {
        return Save(database, Create(database, sequence));
    }

    public static Guid Save(TestDatabase database, Claim claim)
    {
        using var context = database.CreateContext();
        context.Claims.Add(claim);
        context.SaveChanges();

        return claim.Id;
    }

    public static AddPartyInput PartyInput(TestDatabase database, PartyDetails details)
    {
        return new AddPartyInput(details, PolicyPeriod.Empty, _codes, database.Now);
    }

    public static UpdateLossDetailsInput LossInput(TestDatabase database, LossDetails details)
    {
        return new UpdateLossDetailsInput(details, PolicyPeriod.Empty, _codes, database.Now);
    }

    private static PolicyPeriod PeriodOf(Guid policyId)
    {
        return new PolicyPeriod(policyId, new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31), DateOnly.MinValue);
    }
}
