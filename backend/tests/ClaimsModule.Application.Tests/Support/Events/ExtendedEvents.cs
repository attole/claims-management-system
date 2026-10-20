using ClaimsModule.Application.Events.Contracts;
using ClaimsModule.Application.Tests.Support.Claims;
using ClaimsModule.Domain.Common.Events;

namespace ClaimsModule.Application.Tests.Support.Events;

public static class ExtendedEvents
{
    public static ExtendedEvent For(DomainEvent domainEvent)
    {
        return new ExtendedEvent(
            Guid.NewGuid(),
            ClaimHandlerContext.Now,
            ClaimHandlerContext.UserId,
            Guid.NewGuid(),
            domainEvent);
    }
}
