using ClaimsModule.Domain.Claims.RiskObjects;
using ClaimsModule.Domain.Claims.RiskObjects.Operations;
using ClaimsModule.Domain.Tests.Support.Claims;

namespace ClaimsModule.Domain.Tests.Claims.RiskObjects.Operations;

public sealed class AddRiskObjectOperationTests
{
    [Fact]
    [Trait("Rule", "S:RISK-AssetTypes")]
    public void AddRiskObject_AddsItAndTouchesTheClaim()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());

        var van = new RiskObjectDetails(AssetType.Vehicle, "Van");
        var input = new AddRiskObjectInput(van, TestClaims.Actor, TestClaims.Later);
        new AddRiskObjectOperation().Execute(claim, input);

        claim.RiskObjects.Should().HaveCount(2);
        claim.UpdatedAt.Should().Be(TestClaims.Later);
    }
}
