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
    private const int MaxDispatchPasses = 10;

    public async Task CommitAsync(CancellationToken cancellationToken)
    {
        var extendedEvents = await DispatchBeforeCommitAsync(cancellationToken);

        try
        {
            await context.SaveChangesAsync(cancellationToken);
            await CommitTransactionAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException exception)
        {
            throw new ConcurrencyConflictException("The data was changed by someone else.", exception);
        }
        catch (DbUpdateException exception)
        when (exception.InnerException is SqlException { Number: UniqueIndexViolation or UniqueConstraintViolation })
        {
            throw new ConcurrencyConflictException("The data already exists.", exception);
        }

        await dispatcher.DispatchAfterCommitAsync(extendedEvents, cancellationToken);
    }

    private async Task CommitTransactionAsync(CancellationToken cancellationToken)
    {
        var transaction = context.Database.CurrentTransaction;

        if (transaction is not null)
        {
            await transaction.CommitAsync(cancellationToken);
        }
    }

    private async Task<List<ExtendedEvent>> DispatchBeforeCommitAsync(CancellationToken cancellationToken)
    {
        List<ExtendedEvent> extendedEvents = [];

        for (int pass = 0; pass < MaxDispatchPasses; pass++)
        {
            var raised = PopExtendedEvents();

            if (raised.Count == 0)
            {
                return extendedEvents;
            }

            await dispatcher.DispatchBeforeCommitAsync(raised, cancellationToken);
            extendedEvents.AddRange(raised);
        }

        throw new InvalidOperationException("Event handling did not settle.");
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
