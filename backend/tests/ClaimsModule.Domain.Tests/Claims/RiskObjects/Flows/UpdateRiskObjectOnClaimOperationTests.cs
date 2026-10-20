using ClaimsModule.Domain.Claims;
using ClaimsModule.Domain.Claims.RiskObjects;
using ClaimsModule.Domain.Claims.RiskObjects.Flows;
using ClaimsModule.Domain.Claims.RiskObjects.Inputs;
using ClaimsModule.Domain.Claims.RiskObjects.Rules;
using ClaimsModule.Domain.Claims.Rules;
using ClaimsModule.Domain.Tests.Support.Claims;

namespace ClaimsModule.Domain.Tests.Claims.RiskObjects.Flows;

public sealed class UpdateRiskObjectOnClaimOperationTests
{
    [Fact]
    public void Run_ReplacesTheDetails()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());
        var details = new RiskObjectDetails(AssetType.Equipment, "Pump") { DamageDescription = "Seized" };
        var input = new UpdateRiskObjectInput(
            claim.RiskObjects[0].Id,
            details,
            TestClaims.Period(),
            TestClaims.Codes(),
            TestClaims.Start);

        var outcome = UpdateRiskObjectOnClaimOperation.Run(claim, input);

        outcome.Value.Details.Should().Be(details);
    }

    [Fact]
    public void Run_UnknownRiskObject_Throws()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());
        var details = new RiskObjectDetails(AssetType.Equipment, "Pump");
        var input = new UpdateRiskObjectInput(
            Guid.NewGuid(),
            details,
            TestClaims.Period(),
            TestClaims.Codes(),
            TestClaims.Start);

        Action act = () => UpdateRiskObjectOnClaimOperation.Run(claim, input);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    [Trait("Rule", IsClaimEditable.Code)]
    public void Run_WhenTheClaimIsClosed_Fails()
    {
        var claim = TestClaims.InState<ClaimClosedState>();
        var details = new RiskObjectDetails(AssetType.Equipment, "Pump");
        var input = new UpdateRiskObjectInput(
            claim.RiskObjects[0].Id,
            details,
            TestClaims.Period(),
            TestClaims.Codes(),
            TestClaims.Start);

        var outcome = UpdateRiskObjectOnClaimOperation.Run(claim, input);

        outcome.CriticalFindings.Should().ContainSingle().Which.Code.Should().Be(IsClaimEditable.Code);
    }

    [Fact]
    [Trait("Rule", HasNoOtherPrimaryRiskObject.Code)]
    public void Run_ToPrimaryWhenAnotherIsPrimary_Fails()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());
        TestClaims.AddPrimaryRiskObject(claim);
        var details = new RiskObjectDetails(AssetType.Property, "Kitchen") { IsPrimary = true };
        var input = new UpdateRiskObjectInput(
            claim.RiskObjects[0].Id,
            details,
            TestClaims.Period(),
            TestClaims.Codes(),
            TestClaims.Start);

        var outcome = UpdateRiskObjectOnClaimOperation.Run(claim, input);

        outcome.CriticalFindings.Should().ContainSingle().Which.Code.Should().Be(HasNoOtherPrimaryRiskObject.Code);
    }
}
