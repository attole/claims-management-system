using ClaimsModule.Application.Claims.StateChanges;
using ClaimsModule.Application.ReserveComponents.Contracts;
using ClaimsModule.Application.Tests.Support.Claims;
using ClaimsModule.Domain.Claims;

namespace ClaimsModule.Application.Tests.Claims.StateChanges;

public sealed class ClaimStatusContextTests
{
    [Fact]
    public void ToTransitionInput_CarriesTheReasonAndTheNote()
    {
        var command = new ChangeClaimStatusCommand(Guid.NewGuid(), ClaimStateKind.Open)
        {
            ReasonCode = "CODE",
            ReasonText = "text",
            Note = "note",
        };
        var context = ClaimStatusContexts.Create(command);

        var result = context.ToTransitionInput();

        result.Reason.Code.Should().Be("CODE");
        result.Reason.Text.Should().Be("text");
        result.Note.Should().Be("note");
    }

    [Fact]
    public void ToTransitionInput_WithNullText_UsesEmptyText()
    {
        var command = new ChangeClaimStatusCommand(Guid.NewGuid(), ClaimStateKind.Open)
        {
            ReasonCode = null!,
            ReasonText = null!,
            Note = null!,
        };
        var context = ClaimStatusContexts.Create(command);

        var result = context.ToTransitionInput();

        result.Reason.IsSet.Should().BeFalse();
        result.Note.Should().BeEmpty();
    }

    [Fact]
    public void ToFreezeInput_CarriesTheReserveFacts()
    {
        var command = new ChangeClaimStatusCommand(Guid.NewGuid(), ClaimStateKind.Closed);
        var facts = new ClaimReserveFacts(true, false, true, false, 0m);
        var context = ClaimStatusContexts.Create(command, facts);

        var result = context.ToFreezeInput();

        result.HasPendingReserveHistory.Should().BeTrue();
        result.HasUnpostedReserveHistory.Should().BeFalse();
        result.HasOpenReserveBalance.Should().BeTrue();
    }

    [Fact]
    public void ToSendToPaymentInput_CarriesTheApprovedReserveAndTheLiabilityFlag()
    {
        var command = new ChangeClaimStatusCommand(Guid.NewGuid(), ClaimStateKind.PendingPayment)
        {
            IsLiabilityConfirmed = true,
        };
        var facts = new ClaimReserveFacts(false, false, true, true, 100m);
        var context = ClaimStatusContexts.Create(command, facts);

        var result = context.ToSendToPaymentInput();

        result.HasApprovedReserve.Should().BeTrue();
        result.IsLiabilityConfirmed.Should().BeTrue();
    }

    [Fact]
    public void ToReopenInput_CarriesTheActorRole()
    {
        var command = new ChangeClaimStatusCommand(Guid.NewGuid(), ClaimStateKind.Reopened);
        var context = ClaimStatusContexts.Create(command);

        var result = context.ToReopenInput();

        result.ActorRole.Name.Should().Be("Manager");
    }
}
