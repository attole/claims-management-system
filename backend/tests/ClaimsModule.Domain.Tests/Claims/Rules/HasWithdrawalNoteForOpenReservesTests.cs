using ClaimsModule.Domain.Claims.Rules;
using ClaimsModule.Domain.Core.Flows;

namespace ClaimsModule.Domain.Tests.Claims.Rules;

public sealed class HasWithdrawalNoteForOpenReservesTests
{
    [Fact]
    [Trait("Rule", HasWithdrawalNoteForOpenReserves.Code)]
    public void Check_WhenItHolds_Passes()
    {
        var rule = new HasWithdrawalNoteForOpenReserves(true, true);

        var result = rule.Check(NoSubject.Value);

        result.Findings.Should().BeEmpty();
    }

    [Fact]
    [Trait("Rule", HasWithdrawalNoteForOpenReserves.Code)]
    public void Check_WhenItDoesNotHold_ReturnsTheFinding()
    {
        var rule = new HasWithdrawalNoteForOpenReserves(true, false);

        var result = rule.Check(NoSubject.Value);

        result.Findings.Should().ContainSingle().Which.Code.Should().Be(HasWithdrawalNoteForOpenReserves.Code);
    }

    [Fact]
    [Trait("Rule", HasWithdrawalNoteForOpenReserves.Code)]
    public void Check_WhenNoBalanceIsOpen_PassesWithoutANote()
    {
        var rule = new HasWithdrawalNoteForOpenReserves(false, false);

        var result = rule.Check(NoSubject.Value);

        result.Findings.Should().BeEmpty();
    }
}
