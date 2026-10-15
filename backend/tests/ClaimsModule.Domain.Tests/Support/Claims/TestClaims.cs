using ClaimsModule.Domain.Claims.Issues;
using ClaimsModule.Domain.Claims.Issues.Inputs;
using ClaimsModule.Domain.Claims.Loss;
using ClaimsModule.Domain.Claims.Loss.Flows;
using ClaimsModule.Domain.Claims.Loss.Inputs;
using ClaimsModule.Domain.Claims.Parties;
using ClaimsModule.Domain.Claims.Parties.Inputs;
using ClaimsModule.Domain.Claims.RiskObjects;
using ClaimsModule.Domain.Claims.RiskObjects.Inputs;
using ClaimsModule.Domain.Claims.Root;
using ClaimsModule.Domain.Claims.Root.Inputs;
using ClaimsModule.Domain.Common.Roles;

namespace ClaimsModule.Domain.Tests.Support.Claims;

public static class TestClaims
{
    public static readonly DateTimeOffset Start = new(2026, 10, 12, 8, 0, 0, TimeSpan.Zero);
    public static readonly DateTimeOffset Later = Start.AddHours(1);
    public static readonly Guid Organisation = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid Actor = Guid.Parse("22222222-2222-2222-2222-222222222222");
    public const UserRole Handler = UserRole.Handler;
    public const UserRole Supervisor = UserRole.Supervisor;
    public const UserRole Manager = UserRole.Manager;
    public static readonly Guid Policy = Guid.Parse("77777777-7777-7777-7777-777777777777");
    public static readonly DateTimeOffset OutsidePolicyPeriod = new(2025, 12, 31, 8, 0, 0, TimeSpan.Zero);
    public const string Description = "Water damage in the kitchen after a burst pipe";

    public static Claim Empty()
    {
        var claim = WithoutPolicy();
        claim.LinkPolicy(new LinkPolicyInput(Policy, Actor, Start, Reference()));

        return claim;
    }

    public static Claim WithoutPolicy()
    {
        var input = new CreateClaimInput(Organisation, ClaimNumber.From("CLM-2026-0000001"), Actor, Start, Reference());

        return Claim.Create(input).Value;
    }

    public static Claim WithPolicyWarning()
    {
        return WithoutPolicy();
    }

    public static ClaimReferenceSnapshot Reference()
    {
        var period = new PolicyPeriod(new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31), DateOnly.MinValue);

        return new ClaimReferenceSnapshot(["WATER"], period);
    }

    public static Claim Create(params PartyDetails[] parties)
    {
        var claim = Empty();
        var riskObject = new RiskObjectDetails(AssetType.Property, "Kitchen");

        SetLoss(claim, Loss(Start.AddDays(-3), Description));
        claim.AddRiskObject(new AddRiskObjectInput(riskObject, Actor, Start, Reference()));

        foreach (var party in parties)
        {
            claim.AddParty(new AddPartyInput(party, Actor, Start, Reference()));
        }

        claim.PopEvents();

        return claim;
    }

    public static Claim WithLimitOverride()
    {
        var claim = Empty();
        claim.SetLimitOverride(new SetLimitOverrideInput(true, Manager, Actor, Start, Reference()));

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
        var input = new SampleMoveClaimInput(Handler, Actor, Start, Reference());
        SampleMoveClaimTransition<TState>.Execute(claim, input);
        claim.PopEvents();
    }

    public static void ChangeLoss(Claim claim, UpdateLossDetailsInput input)
    {
        var change = new ChangeLossInput(input.Loss, claim.State.Marker, input.Reference, input.Now, input.ActorId);
        UpdateLossDetailsOperation.Execute(claim.LossEvent, change);
    }

    public static ClaimRiskObject AddPrimaryRiskObject(Claim claim)
    {
        var van = new RiskObjectDetails(AssetType.Vehicle, "Van") { IsPrimary = true };
        var input = new AddRiskObjectInput(van, Actor, Start, Reference());

        return claim.AddRiskObject(input).Value;
    }

    public static ClaimReserveSnapshot Reserves()
    {
        return new ClaimReserveSnapshot(1, false, false);
    }

    public static void WaiveIssue(Claim claim, string code)
    {
        var issue = claim.Issues.Single(candidate => candidate.IsCurrent && candidate.Finding.Code == code);
        claim.WaiveIssue(new WaiveIssueInput(issue.Id, Actor, Start, Reference()));
    }

    public static void AcknowledgeIssue(Claim claim, string code)
    {
        var issue = claim.Issues.Single(candidate => candidate.IsCurrent && candidate.Finding.Code == code);
        claim.AcknowledgeIssue(new AcknowledgeIssueInput(issue.Id, Actor, Start, Reference()));
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
        claim.UpdateLossDetails(new UpdateLossDetailsInput(loss, Actor, Start, Reference()));
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
