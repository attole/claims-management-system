using ClaimsModule.Application.Abstractions;
using ClaimsModule.Domain.Common.Entities;
using ClaimsModule.Persistence.Conventions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace ClaimsModule.Persistence.Interceptors;

internal sealed class AuditSoftDeleteInterceptor(IClock clock, ICurrentUser currentUser) : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        Stamp(eventData.Context);

        return result;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        Stamp(eventData.Context);

        return ValueTask.FromResult(result);
    }

    private void Stamp(DbContext? context)
    {
        if (context is null)
        {
            return;
        }

        context.ChangeTracker.DetectChanges();
        var now = clock.UtcNow;
        Guid? actor = currentUser.Id == Guid.Empty ? null : currentUser.Id;
        var entries = context.ChangeTracker.Entries<Entity>()
            .Where(entry => entry.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .ToList();

        foreach (var entry in entries)
        {
            bool hasSoftDelete = EntityTableConvention.HasSoftDelete(entry.Metadata);

            if (entry.State == EntityState.Added)
            {
                entry.Property(RowColumns.CreatedAt).CurrentValue = now;
                entry.Property(RowColumns.UserCreated).CurrentValue = actor;

                continue;
            }

            if (entry.State == EntityState.Deleted)
            {
                if (!hasSoftDelete)
                {
                    throw new InvalidOperationException($"A {entry.Metadata.ClrType.Name} row cannot be deleted.");
                }

                entry.State = EntityState.Modified;
                entry.Property(RowColumns.IsDeleted).CurrentValue = true;
                entry.Property(RowColumns.DeletedAt).CurrentValue = now;
                entry.Property(RowColumns.UpdatedAt).CurrentValue = now;
                entry.Property(RowColumns.UserModified).CurrentValue = actor;

                continue;
            }

            entry.Property(RowColumns.UpdatedAt).CurrentValue = now;
            entry.Property(RowColumns.UserModified).CurrentValue = actor;

            if (hasSoftDelete && entry.Property(RowColumns.IsDeleted) is { IsModified: true, CurrentValue: true })
            {
                entry.Property(RowColumns.DeletedAt).CurrentValue = now;
            }
        }
    }
}
