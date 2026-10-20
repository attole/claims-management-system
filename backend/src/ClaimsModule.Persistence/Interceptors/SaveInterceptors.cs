using ClaimsModule.Application.Abstractions;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace ClaimsModule.Persistence.Interceptors;

internal static class SaveInterceptors
{
    public static IInterceptor[] Create(IClock clock, ICurrentUser currentUser)
    {
        return
        [
            new RootTouchInterceptor(),
            new AppendOnlyInterceptor(),
            new SoftDeleteInterceptor(clock),
            new RowStampInterceptor(clock, currentUser),
        ];
    }
}
