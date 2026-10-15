using ClaimsModule.Domain.Core.Operations;
using ClaimsModule.Domain.ReserveChanges.Root.Rules;
using ClaimsModule.Domain.Reserves.Root;
using ClaimsModule.Domain.Tests.Support.ReserveChanges;

namespace ClaimsModule.Domain.Tests.ReserveChanges.Root.Rules;

public sealed class HasNonNegativeBalanceTests
{
    [Fact]
    [Trait("Rule", HasNonNegativeBalance.Code)]
    public void Check_WhenTheBalanceStaysAtOrAboveZero_Passes()
    {
        var snapshot = TestReserves.Snapshot() with { ReserveApprovedBalance = 1_000m };
        var rule = new HasNonNegativeBalance(snapshot, -1_000m);

        var result = rule.Check(NoSubject.Value);

        result.Findings.Should().BeEmpty();
    }

    [Fact]
    [Trait("Rule", HasNonNegativeBalance.Code)]
    public void Check_WhenTheBalanceWouldGoBelowZero_ReturnsTheFinding()
    {
        var snapshot = TestReserves.Snapshot() with { ReserveApprovedBalance = 1_000m };
        var rule = new HasNonNegativeBalance(snapshot, -1_001m);

        var result = rule.Check(NoSubject.Value);

        result.Findings.Should().ContainSingle().Which.Code.Should().Be(HasNonNegativeBalance.Code);
    }

    [Fact]
    [Trait("Rule", HasNonNegativeBalance.Code)]
    public void Check_ForTheRecoverableType_PassesBelowZero()
    {
        var type = new ReserveSubrogationRecoverableComponentType();
        var snapshot = TestReserves.Snapshot() with { ComponentType = type };
        var rule = new HasNonNegativeBalance(snapshot, -5_000m);

        var result = rule.Check(NoSubject.Value);

        result.Findings.Should().BeEmpty();
    }
}
