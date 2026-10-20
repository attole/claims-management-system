using ClaimsModule.Domain.Claims.Rules;
using ClaimsModule.Domain.Tests.Support.Claims;

namespace ClaimsModule.Domain.Tests.Claims.Rules;

public sealed class HasNoPendingReserveHistoryTests
{
    [Fact]
    [Trait("Rule", HasNoPendingReserveHistory.Code)]
    public void Check_WhenNoReserveHistoryIsPending_Passes()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());
        var rule = new HasNoPendingReserveHistory(false);

        var result = rule.Check(claim);

        result.Findings.Should().BeEmpty();
    }

    [Fact]
    [Trait("Rule", HasNoPendingReserveHistory.Code)]
    public void Check_WhenAReserveHistoryIsPending_ReturnsTheFinding()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());
        var rule = new HasNoPendingReserveHistory(true);

        var result = rule.Check(claim);

        result.Findings.Should().ContainSingle().Which.Code.Should().Be(HasNoPendingReserveHistory.Code);
    }

    [Fact]
    [Trait("Rule", "S:V-Messages")]
    public void Check_WhenAReserveHistoryIsPending_ReturnsTheSpecMessage()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());
        var rule = new HasNoPendingReserveHistory(true);

        var result = rule.Check(claim);

        result.Findings.Should().ContainSingle().Which.Message.Should()
            .StartWith("Claim cannot be closed - ").And.EndWith(" is not satisfied.");
    }
}
