using ClaimsModule.Domain.Claims.RiskObjects.Operations;
using ClaimsModule.Domain.Claims.RiskObjects.Rules;
using ClaimsModule.Domain.Claims.Root;
using ClaimsModule.Domain.Claims.Root.Rules;
using ClaimsModule.Domain.Tests.Support.Claims;

namespace ClaimsModule.Domain.Tests.Claims.RiskObjects.Operations;

public sealed class RemoveRiskObjectOperationTests
{
    [Fact]
    public void RemoveRiskObject_HidesItFromTheClaim()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());
        var input = new RemoveRiskObjectInput(
            claim.RiskObjects[0].Id,
            TestClaims.Actor,
            TestClaims.Later,
            TestClaims.Reference());

        new RemoveRiskObjectOperation().Execute(claim, input);

        claim.RiskObjects.Should().BeEmpty();
    }

    [Fact]
    [Trait("Rule", IsRiskObjectPresent.Code)]
    public void RemoveRiskObject_AlreadyRemoved_FailsAndKeepsTheFirstRemovalTime()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());
        var riskObject = claim.RiskObjects[0];
        var input = new RemoveRiskObjectInput(
            riskObject.Id,
            TestClaims.Actor,
            TestClaims.Later,
            TestClaims.Reference());
        var laterInput = new RemoveRiskObjectInput(
            riskObject.Id,
            TestClaims.Actor,
            TestClaims.Later.AddHours(1),
            TestClaims.Reference());
        new RemoveRiskObjectOperation().Execute(claim, input);

        var outcome = new RemoveRiskObjectOperation().Execute(claim, laterInput);

        outcome.Blocking.Should().ContainSingle().Which.Code.Should().Be(IsRiskObjectPresent.Code);
        riskObject.DeletedAt.Should().Be(TestClaims.Later);
    }

    [Fact]
    [Trait("Rule", IsRiskObjectPresent.Code)]
    public void RemoveRiskObject_UnknownRiskObject_Fails()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());
        var input = new RemoveRiskObjectInput(
            Guid.NewGuid(),
            TestClaims.Actor,
            TestClaims.Later,
            TestClaims.Reference());

        var outcome = new RemoveRiskObjectOperation().Execute(claim, input);

        outcome.Blocking.Should().ContainSingle().Which.Code.Should().Be(IsRiskObjectPresent.Code);
    }

    [Fact]
    [Trait("Rule", IsClaimEditable.Code)]
    public void RemoveRiskObject_WhenTheClaimIsClosed_Fails()
    {
        var claim = TestClaims.InState(new ClaimClosedState());
        var input = new RemoveRiskObjectInput(
            claim.RiskObjects[0].Id,
            TestClaims.Actor,
            TestClaims.Later,
            TestClaims.Reference());

        var outcome = new RemoveRiskObjectOperation().Execute(claim, input);

        outcome.Blocking.Should().ContainSingle().Which.Code.Should().Be(IsClaimEditable.Code);
    }
}
