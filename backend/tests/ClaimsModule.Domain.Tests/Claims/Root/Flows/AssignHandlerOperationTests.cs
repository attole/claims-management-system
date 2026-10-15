using ClaimsModule.Domain.Claims.Root;
using ClaimsModule.Domain.Claims.Root.Flows;
using ClaimsModule.Domain.Claims.Root.Inputs;
using ClaimsModule.Domain.Claims.Root.Rules;
using ClaimsModule.Domain.Tests.Support.Claims;

namespace ClaimsModule.Domain.Tests.Claims.Root.Flows;

public sealed class AssignHandlerOperationTests
{
    [Fact]
    public void AssignHandler_SetsTheHandler()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());
        var handler = Guid.NewGuid();
        var input = new AssignHandlerInput(handler, TestClaims.Actor, TestClaims.Later, TestClaims.Reference());

        AssignHandlerOperation.Execute(claim, input);

        claim.AssignedHandlerId.Should().Be(handler);
    }

    [Fact]
    [Trait("Rule", IsClaimEditable.Code)]
    public void AssignHandler_WhenTheClaimIsWithdrawn_Fails()
    {
        var claim = TestClaims.InState<ClaimWithdrawnState>();
        var input = new AssignHandlerInput(Guid.NewGuid(), TestClaims.Actor, TestClaims.Later, TestClaims.Reference());

        var outcome = AssignHandlerOperation.Execute(claim, input);

        outcome.Blocking.Should().ContainSingle().Which.Code.Should().Be(IsClaimEditable.Code);
    }
}
