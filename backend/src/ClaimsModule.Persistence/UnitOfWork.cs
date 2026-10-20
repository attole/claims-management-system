using ClaimsModule.Application.Abstractions;
using ClaimsModule.Application.Events.Contracts;
using ClaimsModule.Domain.Common;
using ClaimsModule.Domain.Common.Entities;
using ClaimsModule.Domain.Common.Events;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace ClaimsModule.Persistence;

internal sealed class UnitOfWork(
    ClaimsDbContext context,
    IEventDispatcher dispatcher,
    IClock clock,
    ICurrentUser currentUser,
    ICorrelationContext correlation) : IUnitOfWork
{
    private const int UniqueIndexViolation = 2601;
    private const int UniqueConstraintViolation = 2627;

    public async Task CommitAsync(CancellationToken cancellationToken)
    {
        var extendedEvents = await DispatchBeforeCommitAsync(cancellationToken);

        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException exception)
        {
            throw new ConcurrencyConflictException("The data was changed by someone else.", exception);
        }
        catch (DbUpdateException exception) when (IsUniqueViolation(exception))
        {
            throw new ConcurrencyConflictException("The data already exists.", exception);
        }

        await dispatcher.DispatchAfterCommitAsync(extendedEvents, cancellationToken);
    }

    private static bool IsUniqueViolation(DbUpdateException exception)
    {
        return exception.InnerException is SqlException { Number: UniqueIndexViolation or UniqueConstraintViolation };
    }

    private async Task<List<ExtendedEvent>> DispatchBeforeCommitAsync(CancellationToken cancellationToken)
    {
        List<ExtendedEvent> extendedEvents = [];

        while (true)
        {
            var raised = PopExtendedEvents();

            if (raised.Count == 0)
            {
                break;
            }

            await dispatcher.DispatchBeforeCommitAsync(raised, cancellationToken);
            extendedEvents.AddRange(raised);
        }

        return extendedEvents;
    }

    private List<ExtendedEvent> PopExtendedEvents()
    {
        var domainEvents = context.ChangeTracker.Entries<Entity>()
            .ToList()
            .SelectMany(entry => entry.Entity.PopEvents())
            .ToList();
        var extendedEvents = domainEvents.Select(Extend).ToList();

        return extendedEvents;
    }

    private ExtendedEvent Extend(DomainEvent domainEvent)
    {
        return new ExtendedEvent(
            Ids.Next(),
            clock.UtcNow,
            currentUser.Id,
            correlation.CorrelationId,
            domainEvent);
    }
}
