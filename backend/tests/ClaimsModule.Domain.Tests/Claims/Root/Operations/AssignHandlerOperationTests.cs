using ClaimsModule.Domain.Claims.Root;
using ClaimsModule.Domain.Claims.Root.Operations;
using ClaimsModule.Domain.Claims.Root.Rules;
using ClaimsModule.Domain.Tests.Support.Claims;

namespace ClaimsModule.Domain.Tests.Claims.Root.Operations;

public sealed class AssignHandlerOperationTests
{
    [Fact]
    public void AssignHandler_SetsTheHandler()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());
        var handler = Guid.NewGuid();
        var input = new AssignHandlerInput(handler, TestClaims.Actor, TestClaims.Later, TestClaims.Reference());

        new AssignHandlerOperation().Execute(claim, input);

        claim.AssignedHandlerId.Should().Be(handler);
    }

    [Fact]
    [Trait("Rule", IsClaimEditable.Code)]
    public void AssignHandler_WhenTheClaimIsWithdrawn_Fails()
    {
        var claim = TestClaims.InState(new ClaimWithdrawnState());
        var input = new AssignHandlerInput(Guid.NewGuid(), TestClaims.Actor, TestClaims.Later, TestClaims.Reference());

        var outcome = new AssignHandlerOperation().Execute(claim, input);

        outcome.Blocking.Should().ContainSingle().Which.Code.Should().Be(IsClaimEditable.Code);
    }
}
