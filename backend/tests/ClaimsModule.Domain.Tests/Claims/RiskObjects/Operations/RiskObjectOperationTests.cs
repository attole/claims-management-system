using ClaimsModule.Domain.Claims.RiskObjects;
using ClaimsModule.Domain.Claims.RiskObjects.Operations;
using ClaimsModule.Domain.Claims.Root;
using ClaimsModule.Domain.Tests.Support.Claims;

namespace ClaimsModule.Domain.Tests.Claims.RiskObjects.Operations;

public sealed class RiskObjectOperationTests
{
    [Fact]
    [Trait("Rule", "S:RISK-AssetTypes")]
    public void AddRiskObject_AddsItAndTouchesTheClaim()
    {
        Claim claim = TestClaims.Create(TestClaims.Claimant());

        new AddRiskObjectOperation().Execute(
            claim,
            new AddRiskObjectInput(new RiskObjectDetails(AssetType.Vehicle, "Van"), TestClaims.Later));

        claim.RiskObjects.Should().HaveCount(2);
        claim.UpdatedAt.Should().Be(TestClaims.Later);
    }

    [Fact]
    public void RemoveRiskObject_HidesItFromTheClaim()
    {
        Claim claim = TestClaims.Create(TestClaims.Claimant());

        new RemoveRiskObjectOperation().Execute(
            claim,
            new RemoveRiskObjectInput(claim.RiskObjects[0].Id, TestClaims.Later));

        claim.RiskObjects.Should().BeEmpty();
        claim.UpdatedAt.Should().Be(TestClaims.Later);
    }

    [Fact]
    public void RemoveRiskObject_AlreadyRemoved_FailsAndKeepsTheFirstRemovalTime()
    {
        Claim claim = TestClaims.Create(TestClaims.Claimant());
        ClaimRiskObject riskObject = claim.RiskObjects[0];
        RemoveRiskObjectInput input = new(riskObject.Id, TestClaims.Later);
        new RemoveRiskObjectOperation().Execute(claim, input);

        Action removeAgain = () => new RemoveRiskObjectOperation().Execute(
            claim,
            new RemoveRiskObjectInput(riskObject.Id, TestClaims.Later.AddHours(1)));

        removeAgain.Should().Throw<InvalidOperationException>();
        riskObject.DeletedAt.Should().Be(TestClaims.Later);
    }

    [Fact]
    [Trait("Rule", "S:RISK-AssetTypes")]
    public void AssetTypes_AreTheFiveDefinedOnes()
    {
        Enum.GetValues<AssetType>().Should().BeEquivalentTo(
            [AssetType.Vehicle, AssetType.Property, AssetType.Person, AssetType.Equipment, AssetType.Other]);
    }
}
