using ClaimsModule.Domain.Claims.Root.Rules;
using ClaimsModule.Domain.Tests.Support.Claims;

namespace ClaimsModule.Domain.Tests.Claims.Root.Rules;

public sealed class HasClosingNoteForOpenReservesTests
{
    [Fact]
    [Trait("Rule", HasClosingNoteForOpenReserves.Code)]
    public void Check_WhenNoReserveBalanceIsOpen_Passes()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());
        var rule = new HasClosingNoteForOpenReserves(TestClaims.Reserves(), string.Empty);

        var result = rule.Check(claim);

        result.Findings.Should().BeEmpty();
    }

    [Fact]
    [Trait("Rule", HasClosingNoteForOpenReserves.Code)]
    public void Check_WhenABalanceIsOpenAndANoteIsGiven_Passes()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());
        var reserves = TestClaims.Reserves() with { HasOpenReserveBalance = true };
        var rule = new HasClosingNoteForOpenReserves(reserves, "Settled outside the system");

        var result = rule.Check(claim);

        result.Findings.Should().BeEmpty();
    }

    [Fact]
    [Trait("Rule", HasClosingNoteForOpenReserves.Code)]
    public void Check_WhenABalanceIsOpenWithoutANote_ReturnsTheFinding()
    {
        var claim = TestClaims.Create(TestClaims.Claimant());
        var reserves = TestClaims.Reserves() with { HasOpenReserveBalance = true };
        var rule = new HasClosingNoteForOpenReserves(reserves, "  ");

        var result = rule.Check(claim);

        result.Findings.Should().ContainSingle().Which.Code.Should().Be(HasClosingNoteForOpenReserves.Code);
    }
}
