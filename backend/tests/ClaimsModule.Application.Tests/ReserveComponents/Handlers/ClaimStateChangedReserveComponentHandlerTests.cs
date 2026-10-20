using ClaimsModule.Application.ReserveComponents.Handlers;
using ClaimsModule.Application.Tests.Support.Claims;
using ClaimsModule.Application.Tests.Support.Events;
using ClaimsModule.Domain.Claims.Events;
using ClaimsModule.Domain.Claims.StateChanges;
using ClaimsModule.Domain.ReserveComponents;
using ClaimsModule.Domain.ReserveComponents.Inputs;

namespace ClaimsModule.Application.Tests.ReserveComponents.Handlers;

public sealed class ClaimStateChangedReserveComponentHandlerTests
{
    [Theory]
    [Trait("Rule", "S:RES-Components")]
    [InlineData("Closed")]
    [InlineData("Withdrawn")]
    public async Task HandleAsync_WhenTheClaimIsFrozen_ClosesItsActiveComponents(string target)
    {
        var component = OpenComponent();
        var context = new ClaimHandlerContext().WithComponents(component);
        var handler = new ClaimStateChangedReserveComponentHandler(context.ReserveComponents);
        var domainEvent = CreateEvent(component, "Open", target);

        await handler.HandleAsync(domainEvent, ExtendedEvents.For(domainEvent), CancellationToken.None);

        component.State.IsActive.Should().BeFalse();
    }

    [Fact]
    [Trait("Rule", "S:RES-Components")]
    public async Task HandleAsync_WhenTheClaimIsReopened_ReopensItsClosedComponents()
    {
        var component = OpenComponent();
        component.Close();
        var context = new ClaimHandlerContext().WithComponents(component);
        var handler = new ClaimStateChangedReserveComponentHandler(context.ReserveComponents);
        var domainEvent = CreateEvent(component, "Closed", "Reopened");

        await handler.HandleAsync(domainEvent, ExtendedEvents.For(domainEvent), CancellationToken.None);

        component.State.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task HandleAsync_WhenTheClaimMovesBetweenEditableStates_ChangesNothing()
    {
        var component = OpenComponent();
        var context = new ClaimHandlerContext().WithComponents(component);
        var handler = new ClaimStateChangedReserveComponentHandler(context.ReserveComponents);
        var domainEvent = CreateEvent(component, "Open", "UnderInvestigation");

        await handler.HandleAsync(domainEvent, ExtendedEvents.For(domainEvent), CancellationToken.None);

        component.State.IsActive.Should().BeTrue();
    }

    private static ReserveComponent OpenComponent()
    {
        var claim = ApplicationClaims.Opened();
        var input = new OpenReserveComponentInput(claim.Reference, ReserveComponentTypeKind.Indemnity, string.Empty);

        return ReserveComponent.Open(input).Value;
    }

    private static ClaimStateChangedEvent CreateEvent(ReserveComponent component, string from, string to)
    {
        return new ClaimStateChangedEvent(component.ClaimId, from, to, StateChangeReason.Empty);
    }
}
