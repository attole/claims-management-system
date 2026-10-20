using ClaimsModule.Domain.Common.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace ClaimsModule.Persistence.Interceptors;

internal abstract class SaveStepInterceptor : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        Run(eventData.Context);

        return result;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        Run(eventData.Context);

        return ValueTask.FromResult(result);
    }

    protected static List<EntityEntry<Entity>> ChangedEntries(DbContext context)
    {
        return context.ChangeTracker.Entries<Entity>()
            .Where(entry => entry.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .ToList();
    }

    protected abstract void Apply(DbContext context);

    private void Run(DbContext? context)
    {
        if (context is not null)
        {
            Apply(context);
        }
    }
}
