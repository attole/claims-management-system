using ClaimsModule.Domain.Claims.Root;
using ClaimsModule.Domain.Claims.Root.Operations;
using ClaimsModule.Domain.Claims.Root.Rules;
using ClaimsModule.Domain.Tests.Support.Claims;

namespace ClaimsModule.Domain.Tests.Claims.Root.Operations;

public sealed class LinkPolicyOperationTests
{
    [Fact]
    [Trait("Rule", "S:BR-C-06")]
    public void Execute_WithAPolicyId_LinksThePolicy()
    {
        var claim = TestClaims.WithoutPolicy();
        var input = new LinkPolicyInput(TestClaims.Policy, TestClaims.Actor, TestClaims.Later, TestClaims.Reference());

        var outcome = new LinkPolicyOperation().Execute(claim, input);

        outcome.IsSuccess.Should().BeTrue();
        claim.PolicyId.Should().Be(TestClaims.Policy);
    }

    [Fact]
    [Trait("Rule", HasPolicyId.Code)]
    public void Execute_WithoutAPolicyId_ReturnsTheFindingAndKeepsTheClaim()
    {
        var claim = TestClaims.WithoutPolicy();
        var input = new LinkPolicyInput(Guid.Empty, TestClaims.Actor, TestClaims.Later, TestClaims.Reference());

        var outcome = new LinkPolicyOperation().Execute(claim, input);

        outcome.Blocking.Should().ContainSingle().Which.Code.Should().Be(HasPolicyId.Code);
        claim.PolicyId.Should().Be(Guid.Empty);
    }

    [Fact]
    [Trait("Rule", IsClaimEditable.Code)]
    public void Execute_OnAClosedClaim_ReturnsTheFinding()
    {
        var claim = TestClaims.InState(new ClaimClosedState());
        var input = new LinkPolicyInput(TestClaims.Policy, TestClaims.Actor, TestClaims.Later, TestClaims.Reference());

        var outcome = new LinkPolicyOperation().Execute(claim, input);

        outcome.Blocking.Should().ContainSingle().Which.Code.Should().Be(IsClaimEditable.Code);
    }
}
