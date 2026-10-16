using ClaimsModule.Domain.Claims;
using ClaimsModule.Domain.Claims.Flows;
using ClaimsModule.Domain.Claims.Inputs;
using ClaimsModule.Domain.Claims.Rules;
using ClaimsModule.Domain.Tests.Support.Claims;

namespace ClaimsModule.Domain.Tests.Claims.Flows;

public sealed class LinkPolicyOperationTests
{
    [Fact]
    [Trait("Rule", "S:BR-C-06")]
    public void Run_WithAPolicyId_LinksThePolicy()
    {
        var claim = TestClaims.WithoutPolicy();
        var input = new LinkPolicyInput(TestClaims.Policy, TestClaims.LossReference());

        var outcome = LinkPolicyOperation.Run(claim, input);

        outcome.IsSuccess.Should().BeTrue();
        claim.PolicyId.Should().Be(TestClaims.Policy);
    }

    [Fact]
    [Trait("Rule", HasPolicyId.Code)]
    public void Run_WithoutAPolicyId_ReturnsTheFindingAndKeepsTheClaim()
    {
        var claim = TestClaims.WithoutPolicy();
        var input = new LinkPolicyInput(Guid.Empty, TestClaims.LossReference());

        var outcome = LinkPolicyOperation.Run(claim, input);

        outcome.CriticalFindings.Should().ContainSingle().Which.Code.Should().Be(HasPolicyId.Code);
        claim.PolicyId.Should().Be(Guid.Empty);
    }

    [Fact]
    [Trait("Rule", IsClaimEditable.Code)]
    public void Run_OnAClosedClaim_ReturnsTheFinding()
    {
        var claim = TestClaims.InState<ClaimClosedState>();
        var input = new LinkPolicyInput(TestClaims.Policy, TestClaims.LossReference());

        var outcome = LinkPolicyOperation.Run(claim, input);

        outcome.CriticalFindings.Should().ContainSingle().Which.Code.Should().Be(IsClaimEditable.Code);
    }
}
