using ClaimsModule.Domain.Claims;
using ClaimsModule.Domain.Claims.RiskObjects.Flows;
using ClaimsModule.Domain.Claims.RiskObjects.Inputs;
using ClaimsModule.Domain.Claims.Rules;
using ClaimsModule.Domain.Tests.Support.Claims;

namespace ClaimsModule.Domain.Tests.Claims.RiskObjects.Flows;

public sealed class RemoveRiskObjectOnClaimOperationTests
{
    [Fact]
    public void Run_HidesItFromTheClaim()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());
        var input = new RemoveRiskObjectInput(
            claim.RiskObjects[0].Id,
            TestClaims.Later,
            TestClaims.Period(),
            TestClaims.Codes());

        RemoveRiskObjectOnClaimOperation.Run(claim, input);

        claim.HasActiveRiskObject.Should().BeFalse();
    }

    [Fact]
    public void Run_UnknownRiskObject_Throws()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());
        var input = new RemoveRiskObjectInput(
            Guid.NewGuid(),
            TestClaims.Later,
            TestClaims.Period(),
            TestClaims.Codes());

        Action act = () => RemoveRiskObjectOnClaimOperation.Run(claim, input);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    [Trait("Rule", IsClaimEditable.Code)]
    public void Run_WhenTheClaimIsClosed_Fails()
    {
        var claim = TestClaims.InState<ClaimClosedState>();
        var input = new RemoveRiskObjectInput(
            claim.RiskObjects[0].Id,
            TestClaims.Later,
            TestClaims.Period(),
            TestClaims.Codes());

        var outcome = RemoveRiskObjectOnClaimOperation.Run(claim, input);

        outcome.CriticalFindings.Should().ContainSingle().Which.Code.Should().Be(IsClaimEditable.Code);
    }
}
