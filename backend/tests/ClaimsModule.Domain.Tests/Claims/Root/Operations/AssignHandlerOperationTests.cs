using ClaimsModule.Domain.Claims.Root.Operations;
using ClaimsModule.Domain.Tests.Support.Claims;

namespace ClaimsModule.Domain.Tests.Claims.Root.Operations;

public sealed class AssignHandlerOperationTests
{
    [Fact]
    public void AssignHandler_SetsTheHandler()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());
        var handler = Guid.NewGuid();
        var input = new AssignHandlerInput(handler, TestClaims.Actor, TestClaims.Later);

        new AssignHandlerOperation().Execute(claim, input);

        claim.AssignedHandlerId.Should().Be(handler);
        claim.UpdatedAt.Should().Be(TestClaims.Later);
    }
}
