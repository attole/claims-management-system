using ClaimsModule.Domain.Claims;
using ClaimsModule.Domain.Claims.Flows;
using ClaimsModule.Domain.Claims.Inputs;
using ClaimsModule.Domain.Claims.Rules;
using ClaimsModule.Domain.Tests.Support.Claims;

namespace ClaimsModule.Domain.Tests.Claims.Flows;

public sealed class SetSeverityOperationTests
{
    [Fact]
    public void Run_SetsTheSeverity()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());
        var input = new SetSeverityInput(ClaimSeverity.Critical, TestClaims.LossReference());

        SetSeverityOperation.Run(claim, input);

        claim.Severity.Should().Be(ClaimSeverity.Critical);
    }

    [Fact]
    [Trait("Rule", IsClaimEditable.Code)]
    public void Run_WhenTheClaimIsClosed_Fails()
    {
        var claim = TestClaims.InState<ClaimClosedState>();
        var input = new SetSeverityInput(ClaimSeverity.Critical, TestClaims.LossReference());

        var outcome = SetSeverityOperation.Run(claim, input);

        outcome.CriticalFindings.Should().ContainSingle().Which.Code.Should().Be(IsClaimEditable.Code);
    }
}
