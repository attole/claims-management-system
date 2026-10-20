using ClaimsModule.Application.Abstractions;

namespace ClaimsModule.Infrastructure.Clock;

internal sealed class SystemClock : IClock
{
    public DateTimeOffset UtcNow
    {
        get { return DateTimeOffset.UtcNow; }
    }
}
