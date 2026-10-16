using ClaimsModule.Domain.Claims;
using ClaimsModule.Domain.Claims.Rules;
using ClaimsModule.Domain.Tests.Support.Claims;

namespace ClaimsModule.Domain.Tests.Claims.Rules;

public sealed class IsClaimEditableTests
{
    [Fact]
    [Trait("Rule", IsClaimEditable.Code)]
    public void Check_WhenTheClaimIsADraft_Passes()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());

        var result = new IsClaimEditable().Check(claim);

        result.Findings.Should().BeEmpty();
    }

    [Fact]
    [Trait("Rule", IsClaimEditable.Code)]
    public void Check_WhenTheClaimIsClosed_ReturnsTheFinding()
    {
        var claim = TestClaims.InState<ClaimClosedState>();

        var result = new IsClaimEditable().Check(claim);

        result.Findings.Should().ContainSingle().Which.Code.Should().Be(IsClaimEditable.Code);
    }
}
