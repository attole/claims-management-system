using ClaimsModule.Domain.ReserveChanges;
using ClaimsModule.Domain.ReserveChanges.Flows;
using ClaimsModule.Domain.ReserveChanges.Rules;
using ClaimsModule.Domain.Reserves.Rules;
using ClaimsModule.Domain.Tests.Support.Claims;
using ClaimsModule.Domain.Tests.Support.ReserveChanges;

namespace ClaimsModule.Domain.Tests.ReserveChanges.Flows;

public sealed class SubmitReserveOperationTests
{
    [Theory]
    [Trait("Rule", "S:RES-PendingFlow")]
    [InlineData(5_000)]
    [InlineData(50_000)]
    public void Run_AlwaysCreatesAPendingRowWithoutBalances(int amount)
    {
        var input = TestReserves.SubmitInput(amount);

        var outcome = SubmitReserveOperation.Run(input);

        outcome.Value.ApprovalState.Should().BeOfType<ReservePendingApprovalState>();
        outcome.Value.Balances.IsSet.Should().BeFalse();
        outcome.Value.PostingState.Should().BeOfType<ReservePostingPendingState>();
    }

    [Fact]
    [Trait("Rule", "S:RES-PendingFlow")]
    [Trait("Rule", "S:BR-R-06")]
    public void Run_RecordsTheSubmitterTheDetailsAndTheIdempotencyKey()
    {
        var input = TestReserves.SubmitInput(5_000m, 4);

        var outcome = SubmitReserveOperation.Run(input);

        outcome.Value.SubmittedBy.Should().Be(TestReserves.Submitter);
        outcome.Value.Details.ChangeSequence.Should().Be(4);
        outcome.Value.IdempotencyKey.Should().Be($"Reserve:{TestReserves.ReserveId}:Change:4");
    }

    [Fact]
    [Trait("Rule", IsClaimAcceptingReserves.Code)]
    public void Run_OnAClosedOrWithdrawnClaim_ReturnsTheFinding()
    {
        var input = TestReserves.SubmitInput(5_000m) with { IsClaimEditable = false };

        var outcome = SubmitReserveOperation.Run(input);

        outcome.CriticalFindings.Should().ContainSingle().Which.Code.Should().Be(IsClaimAcceptingReserves.Code);
    }

    [Fact]
    public void Run_TakesTheClaimAndTheOrganisationFromTheReserve()
    {
        var input = TestReserves.SubmitInput(5_000m);

        var outcome = SubmitReserveOperation.Run(input);

        outcome.Value.Details.ClaimId.Should().Be(TestReserves.ClaimId);
        outcome.Value.OrganisationId.Should().Be(TestClaims.Organisation);
    }

    [Fact]
    [Trait("Rule", IsReserveAmountNonZero.Code)]
    public void Run_WithAZeroAmount_ReturnsTheFinding()
    {
        var input = TestReserves.SubmitInput(0m);

        var outcome = SubmitReserveOperation.Run(input);

        outcome.IsSuccess.Should().BeFalse();
        outcome.CriticalFindings.Should().ContainSingle().Which.Code.Should().Be(IsReserveAmountNonZero.Code);
    }

    [Fact]
    [Trait("Rule", IsPolicyLinked.Code)]
    public void Run_WithoutALinkedPolicy_ReturnsTheFinding()
    {
        var input = TestReserves.SubmitInput(5_000m) with { IsPolicyLinked = false };

        var outcome = SubmitReserveOperation.Run(input);

        outcome.CriticalFindings.Should().ContainSingle().Which.Code.Should().Be(IsPolicyLinked.Code);
    }

    [Fact]
    [Trait("Rule", HasNonNegativeBalance.Code)]
    public void Run_WhenTheBalanceWouldGoNegative_ReturnsTheFinding()
    {
        var input = TestReserves.SubmitInput(-2_000m) with { ReserveApprovedBalance = 1_000m };

        var outcome = SubmitReserveOperation.Run(input);

        outcome.CriticalFindings.Should().ContainSingle().Which.Code.Should().Be(HasNonNegativeBalance.Code);
    }

    [Fact]
    [Trait("Rule", IsClaimTotalWithinLimit.Code)]
    public void Run_WhenTheClaimTotalWouldPassTheLimit_ReturnsAWarningAndCreatesAPendingRow()
    {
        var input = TestReserves.SubmitInput(8_000m) with { ClaimApprovedTotal = 9_995_000m };

        var outcome = SubmitReserveOperation.Run(input);

        outcome.IsSuccess.Should().BeTrue();
        outcome.WarningFindings.Should().ContainSingle().Which.Code.Should().Be(IsClaimTotalWithinLimit.Code);
        outcome.Value.ApprovalState.Should().BeOfType<ReservePendingApprovalState>();
    }

    [Fact]
    public void Run_RecordsTheSubmissionTime()
    {
        var input = TestReserves.SubmitInput(5_000m);

        var outcome = SubmitReserveOperation.Run(input);

        outcome.Value.SubmittedAt.Should().Be(input.Now);
    }
}
