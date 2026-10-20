using ClaimsModule.Application.Abstractions;
using ClaimsModule.Domain.Common.Capabilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace ClaimsModule.Persistence.Interceptors;

internal sealed class SoftDeleteInterceptor(IClock clock) : SaveStepInterceptor
{
    protected override void Apply(DbContext context)
    {
        var now = clock.UtcNow;
        var entries = ChangedEntries(context).Where(entry => entry.State != EntityState.Added).ToList();

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Deleted)
            {
                if (entry.Entity is not ISoftDeletable)
                {
                    throw new InvalidOperationException($"A {entry.Metadata.ClrType.Name} row cannot be deleted.");
                }

                entry.State = EntityState.Modified;
                entry.Property(RowColumns.IsDeleted).CurrentValue = true;
                entry.Property(RowColumns.DeletedAt).CurrentValue = now;
            }
            else if (IsFlaggedDeleted(entry))
            {
                entry.Property(RowColumns.DeletedAt).CurrentValue = now;
            }
        }
    }

    private static bool IsFlaggedDeleted(EntityEntry entry)
    {
        return entry.Entity is ISoftDeletable
            && entry.Property(RowColumns.IsDeleted) is { IsModified: true, CurrentValue: true };
    }
}
