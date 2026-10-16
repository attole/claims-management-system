using ClaimsModule.Domain.Claims.Rules;
using ClaimsModule.Domain.Tests.Support.Claims;

namespace ClaimsModule.Domain.Tests.Claims.Rules;

public sealed class IsHandlerKeptAfterDraftTests
{
    [Fact]
    [Trait("Rule", IsHandlerKeptAfterDraft.Code)]
    public void Check_WhenItHolds_Passes()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());

        var result = new IsHandlerKeptAfterDraft(Guid.Empty).Check(claim);

        result.Findings.Should().BeEmpty();
    }

    [Fact]
    [Trait("Rule", IsHandlerKeptAfterDraft.Code)]
    public void Check_WhenItDoesNotHold_ReturnsTheFinding()
    {
        var claim = TestClaims.InState<Domain.Claims.ClaimOpenState>();

        var result = new IsHandlerKeptAfterDraft(Guid.Empty).Check(claim);

        result.Findings.Should().ContainSingle().Which.Code.Should().Be(IsHandlerKeptAfterDraft.Code);
    }

    [Fact]
    [Trait("Rule", IsHandlerKeptAfterDraft.Code)]
    public void Check_AfterDraftWithAHandler_Passes()
    {
        var claim = TestClaims.InState<Domain.Claims.ClaimOpenState>();

        var result = new IsHandlerKeptAfterDraft(TestClaims.Actor).Check(claim);

        result.Findings.Should().BeEmpty();
    }
}
