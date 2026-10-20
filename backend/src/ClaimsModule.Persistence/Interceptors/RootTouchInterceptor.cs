using ClaimsModule.Domain.Common.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace ClaimsModule.Persistence.Interceptors;

internal sealed class RootTouchInterceptor : SaveStepInterceptor
{
    protected override void Apply(DbContext context)
    {
        context.ChangeTracker.DetectChanges();

        var rows = context.ChangeTracker.Entries<Entity>().ToList();
        var roots = rows.Where(row => row.Entity is AggregateRoot).ToList();
        var changedRows = new Queue<EntityEntry<Entity>>(ChangedEntries(context));

        while (changedRows.TryDequeue(out var changedRow))
        {
            var parentIds = changedRow.Metadata.GetForeignKeys()
                .Where(key => key.IsMarkedToModifyRoot())
                .Select(key => changedRow.Property(key.Properties[0].Name).CurrentValue)
                .ToList();
            var unchangedParents = roots
                .Where(root => root.State == EntityState.Unchanged)
                .Where(root => parentIds.Contains(root.Entity.Id))
                .ToList();

            foreach (var parent in unchangedParents)
            {
                parent.Property(RowColumns.UpdatedAt).IsModified = true;
                changedRows.Enqueue(parent);
            }
        }
    }
}
