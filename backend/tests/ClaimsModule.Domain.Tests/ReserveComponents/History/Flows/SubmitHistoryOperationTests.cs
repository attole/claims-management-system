using ClaimsModule.Domain.Claims;
using ClaimsModule.Domain.ReserveComponents.History;
using ClaimsModule.Domain.ReserveComponents.History.Flows;
using ClaimsModule.Domain.ReserveComponents.History.Rules;
using ClaimsModule.Domain.ReserveComponents.Rules;
using ClaimsModule.Domain.Tests.Support.Claims;
using ClaimsModule.Domain.Tests.Support.ReserveComponents;

namespace ClaimsModule.Domain.Tests.ReserveComponents.History.Flows;

public sealed class SubmitHistoryOperationTests
{
    [Theory]
    [Trait("Rule", "S:RES-PendingFlow")]
    [InlineData(5_000)]
    [InlineData(50_000)]
    public void Run_AlwaysCreatesAPendingRowWithoutBalances(int amount)
    {
        var component = TestReserves.Component();
        var input = TestReserves.SubmitInput(amount);

        var outcome = SubmitHistoryOperation.Run(component, input);

        outcome.Value.ApprovalState.Should().BeOfType<ReservePendingApprovalState>();
        outcome.Value.Balances.IsSet.Should().BeFalse();
        outcome.Value.PostingState.Should().BeOfType<ReservePostingPendingState>();
    }

    [Fact]
    public void Run_AddsTheRowToTheHistoryOfTheComponent()
    {
        var component = TestReserves.Component();
        var input = TestReserves.SubmitInput(5_000m);

        var outcome = SubmitHistoryOperation.Run(component, input);

        component.History.Should().ContainSingle().Which.Should().BeSameAs(outcome.Value);
    }

    [Fact]
    [Trait("Rule", "S:RES-PendingFlow")]
    [Trait("Rule", "S:BR-R-06")]
    public void Run_RecordsTheSubmitterTheDetailsAndTheIdempotencyKey()
    {
        var component = TestReserves.Component();
        var input = TestReserves.SubmitInput(5_000m);

        var outcome = SubmitHistoryOperation.Run(component, input);

        outcome.Value.SubmittedByUserId.Should().Be(TestReserves.Submitter);
        outcome.Value.Details.ChangeSequence.Should().Be(1);
        outcome.Value.IdempotencyKey.Should().Be($"Reserve:{component.Id}:Change:1");
    }

    [Fact]
    [Trait("Rule", "S:BR-R-06")]
    public void Run_ForASecondRow_TakesTheNextChangeSequence()
    {
        var component = TestReserves.Component();
        var input = TestReserves.SubmitInput(5_000m);
        SubmitHistoryOperation.Run(component, input);

        var outcome = SubmitHistoryOperation.Run(component, input);

        outcome.Value.Details.ChangeSequence.Should().Be(2);
    }

    [Fact]
    [Trait("Rule", IsClaimAcceptingReserves.Code)]
    public void Run_OnAClosedClaim_ReturnsTheFinding()
    {
        var component = TestReserves.Component();
        var input = TestReserves.SubmitInput(5_000m) with { ClaimState = new ClaimClosedState() };

        var outcome = SubmitHistoryOperation.Run(component, input);

        outcome.CriticalFindings.Should().ContainSingle().Which.Code.Should().Be(IsClaimAcceptingReserves.Code);
    }

    [Fact]
    [Trait("Rule", IsReserveComponentActive.Code)]
    public void Run_OnAClosedComponent_ReturnsTheFinding()
    {
        var component = TestReserves.Component();
        component.Close();
        var input = TestReserves.SubmitInput(5_000m);

        var outcome = SubmitHistoryOperation.Run(component, input);

        outcome.CriticalFindings.Should().ContainSingle().Which.Code.Should().Be(IsReserveComponentActive.Code);
    }

    [Fact]
    public void Run_TakesTheClaimAndTheOrganisationFromTheReserveComponent()
    {
        var component = TestReserves.Component();
        var input = TestReserves.SubmitInput(5_000m);

        var outcome = SubmitHistoryOperation.Run(component, input);

        outcome.Value.ReserveComponentId.Should().Be(component.Id);
        outcome.Value.Details.ClaimId.Should().Be(TestReserves.ClaimId);
        outcome.Value.OrganisationId.Should().Be(TestClaims.Organisation);
    }

    [Fact]
    [Trait("Rule", IsReserveAmountNonZero.Code)]
    public void Run_WithAZeroAmount_ReturnsTheFinding()
    {
        var component = TestReserves.Component();
        var input = TestReserves.SubmitInput(0m);

        var outcome = SubmitHistoryOperation.Run(component, input);

        outcome.IsSuccess.Should().BeFalse();
        outcome.CriticalFindings.Should().ContainSingle().Which.Code.Should().Be(IsReserveAmountNonZero.Code);
    }

    [Fact]
    [Trait("Rule", IsPolicyLinked.Code)]
    public void Run_WithoutALinkedPolicy_ReturnsTheFinding()
    {
        var component = TestReserves.Component();
        var input = TestReserves.SubmitInput(5_000m) with { IsPolicyLinked = false };

        var outcome = SubmitHistoryOperation.Run(component, input);

        outcome.CriticalFindings.Should().ContainSingle().Which.Code.Should().Be(IsPolicyLinked.Code);
    }

    [Fact]
    [Trait("Rule", HasNonNegativeBalance.Code)]
    public void Run_WhenTheBalanceWouldGoNegative_ReturnsTheFinding()
    {
        var component = TestReserves.ComponentWithApprovedBalance(1_000m);
        var input = TestReserves.SubmitInput(-2_000m);

        var outcome = SubmitHistoryOperation.Run(component, input);

        outcome.CriticalFindings.Should().ContainSingle().Which.Code.Should().Be(HasNonNegativeBalance.Code);
    }

    [Fact]
    [Trait("Rule", IsClaimTotalWithinLimit.Code)]
    public void Run_WhenTheClaimTotalWouldPassTheLimit_ReturnsAWarningAndCreatesAPendingRow()
    {
        var component = TestReserves.Component();
        var input = TestReserves.SubmitInput(8_000m) with { ClaimApprovedTotal = 9_995_000m };

        var outcome = SubmitHistoryOperation.Run(component, input);

        outcome.IsSuccess.Should().BeTrue();
        outcome.WarningFindings.Should().ContainSingle().Which.Code.Should().Be(IsClaimTotalWithinLimit.Code);
        outcome.Value.ApprovalState.Should().BeOfType<ReservePendingApprovalState>();
    }

    [Fact]
    public void Run_RecordsTheSubmissionTime()
    {
        var component = TestReserves.Component();
        var input = TestReserves.SubmitInput(5_000m);

        var outcome = SubmitHistoryOperation.Run(component, input);

        outcome.Value.SubmittedAt.Should().Be(input.Now);
    }
}
