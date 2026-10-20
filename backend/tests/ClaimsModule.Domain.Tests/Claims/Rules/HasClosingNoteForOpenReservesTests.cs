using ClaimsModule.Domain.Claims.Rules;
using ClaimsModule.Domain.Tests.Support.Claims;

namespace ClaimsModule.Domain.Tests.Claims.Rules;

public sealed class HasClosingNoteForOpenReservesTests
{
    [Fact]
    [Trait("Rule", HasClosingNoteForOpenReserves.Code)]
    public void Check_WhenNoReserveBalanceIsOpen_Passes()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());
        var rule = new HasClosingNoteForOpenReserves(false, false);

        var result = rule.Check(claim);

        result.Findings.Should().BeEmpty();
    }

    [Fact]
    [Trait("Rule", HasClosingNoteForOpenReserves.Code)]
    public void Check_WhenABalanceIsOpenAndANoteIsGiven_Passes()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());
        var rule = new HasClosingNoteForOpenReserves(true, true);

        var result = rule.Check(claim);

        result.Findings.Should().BeEmpty();
    }

    [Fact]
    [Trait("Rule", HasClosingNoteForOpenReserves.Code)]
    public void Check_WhenABalanceIsOpenWithoutANote_ReturnsTheFinding()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());
        var rule = new HasClosingNoteForOpenReserves(true, false);

        var result = rule.Check(claim);

        result.Findings.Should().ContainSingle().Which.Code.Should().Be(HasClosingNoteForOpenReserves.Code);
    }

    [Fact]
    [Trait("Rule", "S:V-Messages")]
    public void Check_WhenABalanceIsOpenWithoutANote_ReturnsTheSpecMessage()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());
        var rule = new HasClosingNoteForOpenReserves(true, false);

        var result = rule.Check(claim);

        result.Findings.Should().ContainSingle().Which.Message.Should()
            .StartWith("Claim cannot be closed - ").And.EndWith(" is not satisfied.");
    }
}
