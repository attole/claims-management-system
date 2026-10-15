using ClaimsModule.Domain.ReserveChanges.Root;
using ClaimsModule.Domain.ReserveChanges.Root.Flows;
using ClaimsModule.Domain.ReserveChanges.Root.Rules;
using ClaimsModule.Domain.Reserves.Root;
using ClaimsModule.Domain.Tests.Support.Claims;
using ClaimsModule.Domain.Tests.Support.ReserveChanges;

namespace ClaimsModule.Domain.Tests.ReserveChanges.Root.Flows;

public sealed class SubmitReserveOperationTests
{
    [Theory]
    [Trait("Rule", "S:RES-PendingFlow")]
    [InlineData(5_000)]
    [InlineData(50_000)]
    public void Execute_AlwaysCreatesAPendingRowWithoutBalances(int amount)
    {
        var input = TestReserves.SubmitInput(amount, TestReserves.Snapshot());

        var outcome = SubmitReserveOperation.Execute(input);

        outcome.Value.ApprovalState.Should().BeOfType<ReservePendingApprovalState>();
        outcome.Value.Balances.IsSet.Should().BeFalse();
        outcome.Value.PostingState.Should().BeOfType<ReservePostingPendingState>();
    }

    [Fact]
    [Trait("Rule", "S:RES-PendingFlow")]
    [Trait("Rule", "S:BR-R-06")]
    public void Execute_RecordsTheSubmitterTheDetailsAndTheIdempotencyKey()
    {
        var snapshot = TestReserves.Snapshot() with { NextChangeSequence = 4 };
        var input = TestReserves.SubmitInput(5_000m, snapshot);

        var outcome = SubmitReserveOperation.Execute(input);

        outcome.Value.SubmittedBy.Should().Be(TestReserves.Submitter);
        outcome.Value.Details.ChangeSequence.Should().Be(4);
        outcome.Value.IdempotencyKey.Should().Be($"Reserve:{TestReserves.ReserveId}:Change:4");
    }

    [Fact]
    [Trait("Rule", IsReserveAmountNonZero.Code)]
    public void Execute_WithAZeroAmount_ReturnsTheFinding()
    {
        var input = TestReserves.SubmitInput(0m, TestReserves.Snapshot());

        var outcome = SubmitReserveOperation.Execute(input);

        outcome.IsSuccess.Should().BeFalse();
        outcome.Blocking.Should().ContainSingle().Which.Code.Should().Be(IsReserveAmountNonZero.Code);
    }

    [Fact]
    [Trait("Rule", IsPolicyLinked.Code)]
    public void Execute_WithoutALinkedPolicy_ReturnsTheFinding()
    {
        var snapshot = TestReserves.Snapshot() with { IsPolicyLinked = false };
        var input = TestReserves.SubmitInput(5_000m, snapshot);

        var outcome = SubmitReserveOperation.Execute(input);

        outcome.Blocking.Should().ContainSingle().Which.Code.Should().Be(IsPolicyLinked.Code);
    }

    [Fact]
    [Trait("Rule", HasNonNegativeBalance.Code)]
    public void Execute_WhenTheBalanceWouldGoNegative_ReturnsTheFinding()
    {
        var snapshot = TestReserves.Snapshot() with { ReserveApprovedBalance = 1_000m };
        var input = TestReserves.SubmitInput(-2_000m, snapshot);

        var outcome = SubmitReserveOperation.Execute(input);

        outcome.Blocking.Should().ContainSingle().Which.Code.Should().Be(HasNonNegativeBalance.Code);
    }

    [Fact]
    [Trait("Rule", "S:RES-Components")]
    public void Execute_ForTheRecoverableType_AllowsANegativeBalance()
    {
        var type = new ReserveSubrogationRecoverableComponentType();
        var snapshot = TestReserves.Snapshot() with { ComponentType = type };
        var input = TestReserves.SubmitInput(-8_000m, snapshot);

        var outcome = SubmitReserveOperation.Execute(input);

        outcome.IsSuccess.Should().BeTrue();
    }

    [Fact]
    [Trait("Rule", IsClaimTotalWithinLimit.Code)]
    public void Execute_WhenTheClaimTotalWouldPassTheLimit_ReturnsAWarningAndCreatesAPendingRow()
    {
        var snapshot = TestReserves.Snapshot() with { ClaimApprovedTotal = 9_995_000m };
        var input = TestReserves.SubmitInput(8_000m, snapshot);

        var outcome = SubmitReserveOperation.Execute(input);

        outcome.IsSuccess.Should().BeTrue();
        outcome.Advisory.Should().ContainSingle().Which.Code.Should().Be(IsClaimTotalWithinLimit.Code);
        outcome.Value.ApprovalState.Should().BeOfType<ReservePendingApprovalState>();
    }

    [Fact]
    [Trait("Rule", "S:RES-History")]
    public void Execute_WithTheTimeOfTheRun_StampsTheCreationTime()
    {
        var input = TestReserves.SubmitInput(5_000m, TestReserves.Snapshot());

        var outcome = SubmitReserveOperation.Execute(input);

        outcome.Value.CreatedAt.Should().Be(TestClaims.Start);
    }
}
