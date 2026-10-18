using ClaimsModule.Domain.ReserveComponents;
using ClaimsModule.Domain.ReserveComponents.Events;
using ClaimsModule.Domain.ReserveComponents.History;
using ClaimsModule.Domain.ReserveComponents.History.Events;
using ClaimsModule.Domain.ReserveComponents.History.Rules;
using ClaimsModule.Domain.Tests.Support.ReserveComponents;

namespace ClaimsModule.Domain.Tests.ReserveComponents;

public sealed class ReserveComponentTests
{
    [Fact]
    [Trait("Rule", "S:BR-R-02")]
    public void SubmitHistory_WithinTheAutoLimit_ApprovesTheRowWithBalances()
    {
        var component = TestReserves.Component();
        var input = TestReserves.SubmitInput(5_000m);

        var outcome = component.SubmitHistory(input);

        outcome.Value.ApprovalState.Should().BeOfType<ReserveAutoApprovedState>();
        outcome.Value.Balances.PreviousBalance.Should().Be(0m);
        outcome.Value.Balances.NewBalance.Should().Be(5_000m);
    }

    [Fact]
    [Trait("Rule", "S:RES-PendingFlow")]
    public void SubmitHistory_AboveTheAutoLimit_LeavesTheRowPending()
    {
        var component = TestReserves.Component();
        var input = TestReserves.SubmitInput(50_000m);

        var outcome = component.SubmitHistory(input);

        outcome.Value.ApprovalState.Should().BeOfType<ReservePendingApprovalState>();
    }

    [Fact]
    [Trait("Rule", "S:RES-History")]
    public void SubmitHistory_RaisesOneSubmittedEventOnTheComponent()
    {
        var component = TestReserves.Component();
        component.PopEvents();
        var input = TestReserves.SubmitInput(50_000m);

        var outcome = component.SubmitHistory(input);

        var submitted = component.PopEvents().OfType<ReserveHistorySubmittedEvent>().Should().ContainSingle().Subject;
        submitted.ReserveComponentId.Should().Be(component.Id);
        submitted.Amount.Should().Be(50_000m);
        submitted.ReserveHistoryId.Should().Be(outcome.Value.Id);
    }

    [Fact]
    [Trait("Rule", "S:RES-History")]
    public void SubmitHistory_WithinTheAutoLimit_RaisesOneApprovalStateChangedEventOnTheRow()
    {
        var component = TestReserves.Component();
        var input = TestReserves.SubmitInput(5_000m);

        var outcome = component.SubmitHistory(input);

        var events = outcome.Value.PopEvents().OfType<ReserveHistoryApprovalStateChangedEvent>();
        var changed = events.Should().ContainSingle().Subject;
        changed.From.Should().Be(nameof(ReserveApprovalStateKind.PendingApproval));
        changed.To.Should().Be(nameof(ReserveApprovalStateKind.AutoApproved));
    }

    [Fact]
    [Trait("Rule", HasNonNegativeBalance.Code)]
    public void SubmitHistory_WhenTheBalanceWouldGoNegative_RaisesNothingAndAddsNoRow()
    {
        var component = TestReserves.Component();
        var input = TestReserves.SubmitInput(-1m);

        component.SubmitHistory(input);

        component.History.Should().BeEmpty();
        component.PopEvents().Should().BeEmpty();
    }

    [Fact]
    public void Close_FromActive_RaisesOneStateChangedEvent()
    {
        var component = TestReserves.Component();
        component.PopEvents();

        component.Close();

        var events = component.PopEvents().OfType<ReserveComponentStateChangedEvent>();
        var changed = events.Should().ContainSingle().Subject;
        changed.From.Should().Be(nameof(ReserveComponentStateKind.Active));
        changed.To.Should().Be(nameof(ReserveComponentStateKind.Closed));
    }

    [Fact]
    public void Reopen_FromClosed_RaisesOneStateChangedEvent()
    {
        var component = TestReserves.Component();
        component.Close();
        component.PopEvents();

        component.Reopen();

        var events = component.PopEvents().OfType<ReserveComponentStateChangedEvent>();
        var changed = events.Should().ContainSingle().Subject;
        changed.To.Should().Be(nameof(ReserveComponentStateKind.Active));
    }

    [Fact]
    public void Reopen_FromActive_RaisesNothing()
    {
        var component = TestReserves.Component();
        component.PopEvents();

        component.Reopen();

        component.PopEvents().Should().BeEmpty();
    }

    [Fact]
    public void ApprovedBalance_SumsTheApprovedRowsOnly()
    {
        var component = TestReserves.ComponentWithApprovedBalance(5_000m);
        var pending = TestReserves.SubmitInput(50_000m);
        component.SubmitHistory(pending);

        var result = component.ApprovedBalance;

        result.Should().Be(5_000m);
    }

    [Fact]
    public void NextChangeSequence_ForAnEmptyComponent_IsOne()
    {
        var component = TestReserves.Component();

        var result = component.NextChangeSequence;

        result.Should().Be(1);
    }

    [Fact]
    public void NextChangeSequence_AfterTwoRows_IsThree()
    {
        var component = TestReserves.Component();
        var input = TestReserves.SubmitInput(1_000m);
        component.SubmitHistory(input);
        component.SubmitHistory(input);

        var result = component.NextChangeSequence;

        result.Should().Be(3);
    }

    [Theory]
    [InlineData(-5_000, true)]
    [InlineData(-5_001, false)]
    public void CanHoldBalanceAfter_ForAnIndemnityComponent_RefusesANegativeBalance(int amount, bool expected)
    {
        var component = TestReserves.ComponentWithApprovedBalance(5_000m);

        var result = component.CanHoldBalanceAfter(amount);

        result.Should().Be(expected);
    }
}
