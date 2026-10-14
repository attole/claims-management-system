using ClaimsModule.Domain.Claims.Loss;
using ClaimsModule.Domain.Claims.Root;
using ClaimsModule.Domain.Claims.Root.Events;
using ClaimsModule.Domain.Claims.Root.Operations;
using ClaimsModule.Domain.Claims.Root.Rules;
using ClaimsModule.Domain.Tests.Support.Claims;

namespace ClaimsModule.Domain.Tests.Claims.Root.Operations;

public sealed class CreateClaimOperationTests
{
    [Fact]
    public void Execute_StartsAsDraftWithUnspecifiedSeverity()
    {
        var claim = TestClaims.Empty();

        claim.State.Should().BeOfType<ClaimDraftState>();
        claim.Severity.Should().Be(ClaimSeverity.Unspecified);
        claim.ReportedDate.Should().Be(TestClaims.Start);
        claim.OrganisationId.Should().Be(TestClaims.Organisation);
        claim.ClaimNumber.Value.Should().Be("CLM-2026-0000001");
    }

    [Fact]
    public void Execute_StartsWithoutPartiesRiskObjectsOrLossDetails()
    {
        var claim = TestClaims.Empty();

        claim.Parties.Should().BeEmpty();
        claim.RiskObjects.Should().BeEmpty();
        claim.ActiveClaimantCount.Should().Be(0);
        claim.LossEvent.ClaimId.Should().Be(claim.Id);
        claim.LossEvent.Details.Should().Be(LossDetails.Empty);
    }

    [Fact]
    public void Execute_RaisesClaimCreated()
    {
        var claim = TestClaims.Empty();

        var created = claim.PopEvents().OfType<ClaimCreated>();

        created.Should().ContainSingle().Which.ClaimId.Should().Be(claim.Id);
    }

    [Fact]
    [Trait("Rule", HasOrganisation.Code)]
    public void Execute_WithoutOrganisation_Fails()
    {
        var number = ClaimNumber.From("CLM-2026-0000001");
        var input = new CreateClaimInput(Guid.Empty, number, TestClaims.Actor, TestClaims.Start);

        var outcome = new CreateClaimOperation().Execute(input);

        outcome.Blocking.Should().ContainSingle().Which.Code.Should().Be(HasOrganisation.Code);
    }

    [Fact]
    [Trait("Rule", HasClaimNumber.Code)]
    public void Execute_WithoutAClaimNumber_Fails()
    {
        var organisation = TestClaims.Organisation;
        var input = new CreateClaimInput(organisation, ClaimNumber.Empty, TestClaims.Actor, TestClaims.Start);

        var outcome = new CreateClaimOperation().Execute(input);

        outcome.Blocking.Should().ContainSingle().Which.Code.Should().Be(HasClaimNumber.Code);
    }

    [Fact]
    public void Execute_ReportsEveryMissingValueAtOnce()
    {
        var input = new CreateClaimInput(Guid.Empty, ClaimNumber.Empty, TestClaims.Actor, TestClaims.Start);

        var outcome = new CreateClaimOperation().Execute(input);

        outcome.Blocking.Select(finding => finding.Code).Should().BeEquivalentTo(
            [HasOrganisation.Code, HasClaimNumber.Code]);
    }

    [Fact]
    public void Execute_WithoutATime_IsRefusedByTheInput()
    {
        var number = ClaimNumber.From("CLM-2026-0000001");

        Func<CreateClaimInput> act = () => new CreateClaimInput(Guid.NewGuid(), number, TestClaims.Actor, default);

        act.Should().Throw<ArgumentException>();
    }
}
