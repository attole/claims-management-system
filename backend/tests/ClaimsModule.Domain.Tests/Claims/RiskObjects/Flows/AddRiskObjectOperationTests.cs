using ClaimsModule.Domain.Claims;
using ClaimsModule.Domain.Claims.RiskObjects;
using ClaimsModule.Domain.Claims.RiskObjects.Flows;
using ClaimsModule.Domain.Claims.RiskObjects.Inputs;
using ClaimsModule.Domain.Claims.RiskObjects.Rules;
using ClaimsModule.Domain.Claims.Rules;
using ClaimsModule.Domain.Tests.Support.Claims;

namespace ClaimsModule.Domain.Tests.Claims.RiskObjects.Flows;

public sealed class AddRiskObjectOperationTests
{
    [Fact]
    [Trait("Rule", "S:RISK-AssetTypes")]
    public void Run_AddsIt()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());
        var van = new RiskObjectDetails(AssetType.Vehicle, "Van");
        var input = new AddRiskObjectInput(van, TestClaims.Period(), TestClaims.Codes(), TestClaims.Start);

        AddRiskObjectOperation.Run(claim, input);

        claim.RiskObjects.Should().HaveCount(2);
    }

    [Fact]
    [Trait("Rule", IsClaimEditable.Code)]
    public void Run_WhenTheClaimIsClosed_Fails()
    {
        var claim = TestClaims.InState<ClaimClosedState>();
        var van = new RiskObjectDetails(AssetType.Vehicle, "Van");
        var input = new AddRiskObjectInput(van, TestClaims.Period(), TestClaims.Codes(), TestClaims.Start);

        var outcome = AddRiskObjectOperation.Run(claim, input);

        outcome.CriticalFindings.Should().ContainSingle().Which.Code.Should().Be(IsClaimEditable.Code);
    }

    [Fact]
    [Trait("Rule", HasNoOtherPrimaryRiskObject.Code)]
    public void Run_AsPrimaryWhenAnotherIsPrimary_Fails()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());
        TestClaims.AddPrimaryRiskObject(claim);
        var truck = new RiskObjectDetails(AssetType.Vehicle, "Truck") { IsPrimary = true };
        var input = new AddRiskObjectInput(truck, TestClaims.Period(), TestClaims.Codes(), TestClaims.Start);

        var outcome = AddRiskObjectOperation.Run(claim, input);

        outcome.CriticalFindings.Should().ContainSingle().Which.Code.Should().Be(HasNoOtherPrimaryRiskObject.Code);
    }
}
