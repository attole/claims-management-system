using ClaimsModule.Domain.Claims.Loss;
using ClaimsModule.Domain.Claims.Root;
using ClaimsModule.Domain.Claims.Root.Events;
using ClaimsModule.Domain.Tests.Support.Claims;

namespace ClaimsModule.Domain.Tests.Claims.Root;

public sealed class ClaimCreateTests
{
    [Fact]
    public void Create_StartsAsDraftWithUnspecifiedSeverity()
    {
        Claim claim = TestClaims.Empty();

        claim.State.Should().BeOfType<DraftState>();
        claim.Severity.Should().Be(ClaimSeverity.Unspecified);
        claim.ReportedDate.Should().Be(TestClaims.Start);
        claim.OrganisationId.Should().Be(TestClaims.Organisation);
        claim.ClaimNumber.Value.Should().Be("CLM-2026-0000001");
    }

    [Fact]
    public void Create_StartsWithoutPartiesRiskObjectsOrLossDetails()
    {
        Claim claim = TestClaims.Empty();

        claim.Parties.Should().BeEmpty();
        claim.RiskObjects.Should().BeEmpty();
        claim.ActiveClaimantCount.Should().Be(0);
        claim.LossEvent.ClaimId.Should().Be(claim.Id);
        claim.LossEvent.Details.Should().Be(LossDetails.Empty);
    }

    [Fact]
    public void Create_RaisesClaimCreated()
    {
        Claim claim = TestClaims.Empty();

        claim.PopEvents().Should().ContainSingle()
            .Which.Should().BeOfType<ClaimCreated>()
            .Which.ClaimId.Should().Be(claim.Id);
    }

    [Fact]
    public void Create_WithoutOrganisation_Throws()
    {
        Action create = () => Claim.Create(
            Guid.Empty,
            ClaimNumber.From("CLM-2026-0000001"),
            TestClaims.Actor,
            TestClaims.Start);

        create.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithoutAClaimNumber_Throws()
    {
        Action create = () => Claim.Create(
            TestClaims.Organisation,
            ClaimNumber.Empty,
            TestClaims.Actor,
            TestClaims.Start);

        create.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithoutATime_Throws()
    {
        Action create = () => Claim.Create(
            TestClaims.Organisation,
            ClaimNumber.From("CLM-2026-0000001"),
            TestClaims.Actor,
            default);

        create.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void TheFixtureFillsTheClaimThroughTheOperations()
    {
        Claim claim = TestClaims.Create(TestClaims.Claimant(), TestClaims.Witness());

        claim.Parties.Select(party => party.Details).Should().Equal(TestClaims.Claimant(), TestClaims.Witness());
        claim.RiskObjects.Should().ContainSingle();
        claim.LossEvent.Details.Description.Should().Be("Water damage");
        claim.ActiveClaimantCount.Should().Be(1);
    }
}
