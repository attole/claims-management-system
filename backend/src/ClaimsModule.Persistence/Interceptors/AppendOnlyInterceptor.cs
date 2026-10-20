using ClaimsModule.Domain.Common.Capabilities;
using Microsoft.EntityFrameworkCore;

namespace ClaimsModule.Persistence.Interceptors;

internal sealed class AppendOnlyInterceptor : SaveStepInterceptor
{
    protected override void Apply(DbContext context)
    {
        bool isChanged = ChangedEntries(context)
            .Any(entry => entry.Entity is IAppendOnly && entry.State != EntityState.Added);

        if (isChanged)
        {
            throw new InvalidOperationException("An append-only row cannot be changed or removed.");
        }
    }
}
