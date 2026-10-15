using ClaimsModule.Domain.Claims.Root;
using ClaimsModule.Domain.Claims.Root.Flows;
using ClaimsModule.Domain.Claims.Root.Inputs;
using ClaimsModule.Domain.Claims.Root.Rules;
using ClaimsModule.Domain.Tests.Support.Claims;

namespace ClaimsModule.Domain.Tests.Claims.Root.Flows;

public sealed class SetSeverityOperationTests
{
    [Fact]
    public void SetSeverity_SetsTheSeverity()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());
        var input = new SetSeverityInput(
            ClaimSeverity.Critical,
            TestClaims.Actor,
            TestClaims.Later,
            TestClaims.Reference());

        SetSeverityOperation.Execute(claim, input);

        claim.Severity.Should().Be(ClaimSeverity.Critical);
    }

    [Fact]
    [Trait("Rule", IsClaimEditable.Code)]
    public void SetSeverity_WhenTheClaimIsClosed_Fails()
    {
        var claim = TestClaims.InState<ClaimClosedState>();
        var input = new SetSeverityInput(
            ClaimSeverity.Critical,
            TestClaims.Actor,
            TestClaims.Later,
            TestClaims.Reference());

        var outcome = SetSeverityOperation.Execute(claim, input);

        outcome.Blocking.Should().ContainSingle().Which.Code.Should().Be(IsClaimEditable.Code);
    }
}
