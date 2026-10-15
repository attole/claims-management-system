using ClaimsModule.Domain.Claims.RiskObjects;
using ClaimsModule.Domain.Claims.RiskObjects.Flows;
using ClaimsModule.Domain.Claims.RiskObjects.Inputs;
using ClaimsModule.Domain.Claims.RiskObjects.Rules;
using ClaimsModule.Domain.Claims.Root;
using ClaimsModule.Domain.Claims.Root.Rules;
using ClaimsModule.Domain.Tests.Support.Claims;

namespace ClaimsModule.Domain.Tests.Claims.RiskObjects.Flows;

public sealed class UpdateRiskObjectOperationTests
{
    [Fact]
    public void UpdateRiskObject_ReplacesTheDetails()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());
        var details = new RiskObjectDetails(AssetType.Equipment, "Pump") { DamageDescription = "Seized" };
        var input = new UpdateRiskObjectInput(
            claim.RiskObjects[0].Id,
            details,
            TestClaims.Actor,
            TestClaims.Later,
            TestClaims.Reference());

        var outcome = UpdateRiskObjectOperation.Execute(claim, input);

        outcome.Value.Details.Should().Be(details);
        outcome.Value.UpdatedAt.Should().Be(TestClaims.Later);
    }

    [Fact]
    [Trait("Rule", IsRiskObjectPresent.Code)]
    public void UpdateRiskObject_UnknownRiskObject_Fails()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());
        var details = new RiskObjectDetails(AssetType.Equipment, "Pump");
        var input = new UpdateRiskObjectInput(
            Guid.NewGuid(),
            details,
            TestClaims.Actor,
            TestClaims.Later,
            TestClaims.Reference());

        var outcome = UpdateRiskObjectOperation.Execute(claim, input);

        outcome.Blocking.Should().ContainSingle().Which.Code.Should().Be(IsRiskObjectPresent.Code);
    }

    [Fact]
    [Trait("Rule", IsRiskObjectPresent.Code)]
    public void UpdateRiskObject_ARemovedRiskObject_Fails()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());
        var riskObjectId = claim.RiskObjects[0].Id;
        var remove = new RemoveRiskObjectInput(
            riskObjectId,
            TestClaims.Actor,
            TestClaims.Later,
            TestClaims.Reference());
        RemoveRiskObjectOperation.Execute(claim, remove);
        var details = new RiskObjectDetails(AssetType.Equipment, "Pump");
        var input = new UpdateRiskObjectInput(
            riskObjectId,
            details,
            TestClaims.Actor,
            TestClaims.Later,
            TestClaims.Reference());

        var outcome = UpdateRiskObjectOperation.Execute(claim, input);

        outcome.Blocking.Should().ContainSingle().Which.Code.Should().Be(IsRiskObjectPresent.Code);
    }

    [Fact]
    [Trait("Rule", IsClaimEditable.Code)]
    public void UpdateRiskObject_WhenTheClaimIsClosed_Fails()
    {
        var claim = TestClaims.InState<ClaimClosedState>();
        var details = new RiskObjectDetails(AssetType.Equipment, "Pump");
        var input = new UpdateRiskObjectInput(
            claim.RiskObjects[0].Id,
            details,
            TestClaims.Actor,
            TestClaims.Later,
            TestClaims.Reference());

        var outcome = UpdateRiskObjectOperation.Execute(claim, input);

        outcome.Blocking.Should().ContainSingle().Which.Code.Should().Be(IsClaimEditable.Code);
    }

    [Fact]
    [Trait("Rule", HasNoOtherPrimaryRiskObject.Code)]
    public void UpdateRiskObject_ToPrimaryWhenAnotherIsPrimary_Fails()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());
        TestClaims.AddPrimaryRiskObject(claim);
        var details = new RiskObjectDetails(AssetType.Property, "Kitchen") { IsPrimary = true };
        var input = new UpdateRiskObjectInput(
            claim.RiskObjects[0].Id,
            details,
            TestClaims.Actor,
            TestClaims.Later,
            TestClaims.Reference());

        var outcome = UpdateRiskObjectOperation.Execute(claim, input);

        outcome.Blocking.Should().ContainSingle().Which.Code.Should().Be(HasNoOtherPrimaryRiskObject.Code);
    }

    [Fact]
    [Trait("Rule", HasNoOtherPrimaryRiskObject.Code)]
    public void UpdateRiskObject_OfThePrimaryItself_KeepsItPrimary()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());
        var primary = TestClaims.AddPrimaryRiskObject(claim);
        var details = new RiskObjectDetails(AssetType.Vehicle, "Red van") { IsPrimary = true };
        var input = new UpdateRiskObjectInput(
            primary.Id,
            details,
            TestClaims.Actor,
            TestClaims.Later,
            TestClaims.Reference());

        var outcome = UpdateRiskObjectOperation.Execute(claim, input);

        outcome.Value.Details.IsPrimary.Should().BeTrue();
    }

    [Fact]
    [Trait("Rule", HasNoOtherPrimaryRiskObject.Code)]
    public void UpdateRiskObject_AfterTheOldPrimaryIsDemoted_MakesAnotherPrimary()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());
        var primary = TestClaims.AddPrimaryRiskObject(claim);
        var demoted = new RiskObjectDetails(AssetType.Vehicle, "Van") { IsPrimary = false };
        var demote = new UpdateRiskObjectInput(
            primary.Id,
            demoted,
            TestClaims.Actor,
            TestClaims.Later,
            TestClaims.Reference());
        UpdateRiskObjectOperation.Execute(claim, demote);
        var promoted = new RiskObjectDetails(AssetType.Property, "Kitchen") { IsPrimary = true };
        var promote = new UpdateRiskObjectInput(
            claim.RiskObjects[0].Id,
            promoted,
            TestClaims.Actor,
            TestClaims.Later,
            TestClaims.Reference());

        var outcome = UpdateRiskObjectOperation.Execute(claim, promote);

        outcome.Value.Details.IsPrimary.Should().BeTrue();
    }
}
