using ClaimsModule.Domain.Claims;
using ClaimsModule.Domain.Claims.Flows;
using ClaimsModule.Domain.Claims.Inputs;
using ClaimsModule.Domain.Claims.Rules;
using ClaimsModule.Domain.Tests.Support.Claims;

namespace ClaimsModule.Domain.Tests.Claims.Flows;

public sealed class AssignHandlerOperationTests
{
    [Fact]
    public void Run_SetsTheHandler()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());
        var handler = Guid.NewGuid();
        var input = new AssignHandlerInput(handler, TestClaims.Period(), TestClaims.Codes());

        AssignHandlerOperation.Run(claim, input);

        claim.AssignedHandlerId.Should().Be(handler);
    }

    [Fact]
    [Trait("Rule", IsClaimEditable.Code)]
    public void Run_WhenTheClaimIsWithdrawn_Fails()
    {
        var claim = TestClaims.InState<ClaimWithdrawnState>();
        var input = new AssignHandlerInput(Guid.NewGuid(), TestClaims.Period(), TestClaims.Codes());

        var outcome = AssignHandlerOperation.Run(claim, input);

        outcome.CriticalFindings.Should().ContainSingle().Which.Code.Should().Be(IsClaimEditable.Code);
    }

    [Fact]
    [Trait("Rule", IsHandlerKeptAfterDraft.Code)]
    public void Run_UnassigningAfterDraft_ReturnsTheFindingAndKeepsTheHandler()
    {
        var claim = TestClaims.InState<ClaimOpenState>();
        var input = new AssignHandlerInput(Guid.Empty, TestClaims.Period(), TestClaims.Codes());

        var outcome = AssignHandlerOperation.Run(claim, input);

        outcome.CriticalFindings.Should().ContainSingle().Which.Code.Should().Be(IsHandlerKeptAfterDraft.Code);
        claim.AssignedHandlerId.Should().Be(TestClaims.Actor);
    }
}
