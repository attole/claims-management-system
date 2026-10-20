using ClaimsModule.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace ClaimsModule.Persistence.Interceptors;

internal sealed class RowStampInterceptor(IClock clock, ICurrentUser currentUser) : SaveStepInterceptor
{
    protected override void Apply(DbContext context)
    {
        var now = clock.UtcNow;
        Guid? actor = currentUser.Id == Guid.Empty ? null : currentUser.Id;

        foreach (var entry in ChangedEntries(context))
        {
            if (entry.State == EntityState.Added)
            {
                entry.Property(RowColumns.CreatedAt).CurrentValue = now;
                entry.Property(RowColumns.UserCreated).CurrentValue = actor;
            }
            else
            {
                entry.Property(RowColumns.UpdatedAt).CurrentValue = now;
                entry.Property(RowColumns.UserModified).CurrentValue = actor;
            }
        }
    }
}
