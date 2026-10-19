using ClaimsModule.Domain.CauseOfLoss;
using ClaimsModule.Domain.Claims;
using ClaimsModule.Domain.Claims.Inputs;
using ClaimsModule.Domain.Claims.Issues.Inputs;
using ClaimsModule.Domain.Claims.Loss;
using ClaimsModule.Domain.Claims.Loss.Inputs;
using ClaimsModule.Domain.Claims.Parties;
using ClaimsModule.Domain.Claims.Parties.Inputs;
using ClaimsModule.Domain.Claims.RiskObjects;
using ClaimsModule.Domain.Claims.RiskObjects.Inputs;
using ClaimsModule.Domain.Policies;
using ClaimsModule.Domain.Roles;

namespace ClaimsModule.Domain.Tests.Support.Claims;

public static class TestClaims
{
    public const string Description = "Water damage in the kitchen after a burst pipe";

    public static readonly DateTimeOffset Start = new(2026, 10, 12, 8, 0, 0, TimeSpan.Zero);
    public static readonly DateTimeOffset Later = Start.AddHours(1);
    public static readonly DateOnly ReportDay = DateOnly.FromDateTime(Start.UtcDateTime);
    public static readonly Guid Organisation = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid Actor = Guid.Parse("22222222-2222-2222-2222-222222222222");
    public static readonly UserRole Handler = new UserHandlerRole();
    public static readonly UserRole Supervisor = new UserSupervisorRole();
    public static readonly UserRole Manager = new UserManagerRole();
    public static readonly Guid Policy = Guid.Parse("77777777-7777-7777-7777-777777777777");
    public static readonly DateTimeOffset OutsidePolicyPeriod = new(2025, 12, 31, 8, 0, 0, TimeSpan.Zero);

    public static Claim Empty()
    {
        var claim = WithoutPolicy();
        claim.LinkPolicy(new LinkPolicyInput(Policy, Period(), Codes()));

        return claim;
    }

    public static Claim WithoutPolicy()
    {
        var input = new CreateClaimInput(
            Organisation,
            ClaimNumber.From("CLM-2026-0000001"),
            Start,
            Start,
            PolicyPeriod.Empty,
            Codes());

        return Claim.Create(input).Value;
    }

    public static PolicyPeriod Period()
    {
        return new PolicyPeriod(Policy, new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31), DateOnly.MinValue);
    }

    public static CauseCodes Codes()
    {
        return new CauseCodes(["WATER"]);
    }

    public static CauseCodes CodesOf(params string[] codes)
    {
        return new CauseCodes(codes);
    }

    public static Claim Create(params PartyDetails[] parties)
    {
        var claim = Empty();
        var riskObject = new RiskObjectDetails(AssetType.Property, "Kitchen");

        SetLoss(claim, Loss(Start.AddDays(-3), Description));
        claim.AddRiskObject(new AddRiskObjectInput(riskObject, Period(), Codes()));
        claim.AssignHandler(new AssignHandlerInput(Actor, Period(), Codes()));

        foreach (var party in parties)
        {
            claim.AddParty(new AddPartyInput(party, Period(), Codes()));
        }

        claim.PopEvents();

        return claim;
    }

    public static Claim WithLimitOverride()
    {
        var claim = Empty();
        claim.SetLimitOverride(new SetLimitOverrideInput(true, Manager, 0m, Period(), Codes()));

        return claim;
    }

    public static Claim InState<TState>()
        where TState : ClaimState, new()
    {
        var claim = Create(Claimant());
        MoveTo<TState>(claim);

        return claim;
    }

    public static Claim InStateWithoutClaimant<TState>()
        where TState : ClaimState, new()
    {
        var claim = Create();
        MoveTo<TState>(claim);

        return claim;
    }

    public static void MoveTo<TState>(Claim claim)
        where TState : ClaimState, new()
    {
        var input = new ClaimTransitionInput(Period(), Codes());
        SampleMoveClaimTransition<TState>.Run(claim, input);
        claim.PopEvents();
    }

    public static void ChangeLoss(Claim claim, UpdateLossDetailsInput input)
    {
        claim.LossEvent.Update(new LossDetailsInput(input.Loss));
    }

    public static ClaimRiskObject AddPrimaryRiskObject(Claim claim)
    {
        var van = new RiskObjectDetails(AssetType.Vehicle, "Van") { IsPrimary = true };
        var input = new AddRiskObjectInput(van, Period(), Codes());

        return claim.AddRiskObject(input).Value;
    }

    public static void WaiveIssue(Claim claim, string code)
    {
        var issue = claim.Issues.Single(issue => issue.State.IsCurrent && issue.Finding.HasCode(code));
        claim.WaiveIssue(new WaiveIssueInput(issue.Id, Supervisor, Actor, Start, Period(), Codes()));
    }

    public static void AcknowledgeIssue(Claim claim, string code)
    {
        var issue = claim.Issues.Single(issue => issue.State.IsCurrent && issue.Finding.HasCode(code));
        claim.AcknowledgeIssue(new AcknowledgeIssueInput(issue.Id, Actor, Start, Period(), Codes()));
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
        claim.UpdateLossDetails(new UpdateLossDetailsInput(loss, Period(), Codes()));
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
