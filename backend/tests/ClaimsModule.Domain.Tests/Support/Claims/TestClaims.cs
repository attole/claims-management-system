using ClaimsModule.Domain.Claims.Issues;
using ClaimsModule.Domain.Claims.Issues.Operations;
using ClaimsModule.Domain.Claims.Loss;
using ClaimsModule.Domain.Claims.Loss.Operations;
using ClaimsModule.Domain.Claims.Parties;
using ClaimsModule.Domain.Claims.Parties.Operations;
using ClaimsModule.Domain.Claims.RiskObjects;
using ClaimsModule.Domain.Claims.RiskObjects.Operations;
using ClaimsModule.Domain.Claims.Root;
using ClaimsModule.Domain.Claims.Root.Operations;
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
    public const string Description = "Water damage in the kitchen after a burst pipe";

    public static Claim Empty()
    {
        var input = new CreateClaimInput(Organisation, ClaimNumber.From("CLM-2026-0000001"), Actor, Start);

        var claim = new CreateClaimOperation().Execute(input).Value;
        Evaluate(claim);

        return claim;
    }

    public static void Evaluate(Claim claim)
    {
        var input = new EvaluateIssuesInput(Start, Actor);
        new EvaluateIssuesFlow().Execute(claim, input);
    }

    public static Claim Create(params PartyDetails[] parties)
    {
        var claim = Empty();
        var riskObject = new RiskObjectDetails(AssetType.Property, "Kitchen");

        SetLoss(claim, Loss(Start.AddDays(-3), Description));
        new AddRiskObjectOperation().Execute(claim, new AddRiskObjectInput(riskObject, Actor, Start));
        Evaluate(claim);

        foreach (var party in parties)
        {
            new AddPartyOperation().Execute(claim, new AddPartyInput(party, Actor, Start));
            Evaluate(claim);
        }

        claim.PopEvents();

        return claim;
    }

    public static Claim InState(ClaimState state)
    {
        var claim = Create(Claimant());
        var input = new SampleMoveClaimInput(Handler, Actor, Start);
        new SampleMoveClaimTransition(state).Execute(claim, input);
        claim.PopEvents();

        return claim;
    }

    public static void WaiveIssue(Claim claim, string code)
    {
        var issue = claim.Issues.Single(candidate => candidate.IsCurrent && candidate.Finding.Code == code);
        var input = new WaiveIssueInput(issue.Id, Actor, Start);
        new WaiveIssueOperation().Execute(claim, input);
        Evaluate(claim);
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
        Evaluate(claim);
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
