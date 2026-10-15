using ClaimsModule.Domain.Claims.RiskObjects;
using ClaimsModule.Domain.Claims.RiskObjects.Operations;
using ClaimsModule.Domain.Claims.Root;
using ClaimsModule.Domain.Claims.Root.Rules;
using ClaimsModule.Domain.Tests.Support.Claims;

namespace ClaimsModule.Domain.Tests.Claims.RiskObjects.Operations;

public sealed class AddRiskObjectOperationTests
{
    [Fact]
    [Trait("Rule", "S:RISK-AssetTypes")]
    public void AddRiskObject_AddsIt()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());

        var van = new RiskObjectDetails(AssetType.Vehicle, "Van");
        var input = new AddRiskObjectInput(van, TestClaims.Actor, TestClaims.Later, TestClaims.Reference());
        new AddRiskObjectOperation().Execute(claim, input);

        claim.RiskObjects.Should().HaveCount(2);
    }

    [Fact]
    [Trait("Rule", IsClaimEditable.Code)]
    public void AddRiskObject_WhenTheClaimIsClosed_Fails()
    {
        var claim = TestClaims.InState(new ClaimClosedState());
        var van = new RiskObjectDetails(AssetType.Vehicle, "Van");
        var input = new AddRiskObjectInput(van, TestClaims.Actor, TestClaims.Later, TestClaims.Reference());

        var outcome = new AddRiskObjectOperation().Execute(claim, input);

        outcome.Blocking.Should().ContainSingle().Which.Code.Should().Be(IsClaimEditable.Code);
    }
}
