using ClaimsModule.Domain.Claims.Root;
using ClaimsModule.Domain.Claims.Root.Operations;
using ClaimsModule.Domain.Claims.Root.Rules;
using ClaimsModule.Domain.Tests.Support.Claims;

namespace ClaimsModule.Domain.Tests.Claims.Root.Operations;

public sealed class SetSeverityOperationTests
{
    [Fact]
    public void SetSeverity_SetsTheSeverity()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());
        var input = new SetSeverityInput(ClaimSeverity.Critical, TestClaims.Actor, TestClaims.Later);

        new SetSeverityOperation().Execute(claim, input);

        claim.Severity.Should().Be(ClaimSeverity.Critical);
        claim.UpdatedAt.Should().Be(TestClaims.Later);
    }

    [Fact]
    [Trait("Rule", IsClaimEditable.Code)]
    public void SetSeverity_WhenTheClaimIsClosed_Fails()
    {
        var claim = TestClaims.InState(new ClaimClosedState());
        var input = new SetSeverityInput(ClaimSeverity.Critical, TestClaims.Actor, TestClaims.Later);

        var outcome = new SetSeverityOperation().Execute(claim, input);

        outcome.Blocking.Should().ContainSingle().Which.Code.Should().Be(IsClaimEditable.Code);
    }
}
