namespace ClaimsModule.Domain.Tests.Support.Engine;

public sealed class CountingTimeProvider(DateTimeOffset now) : TimeProvider
{
    public int Calls { get; private set; }

    public override DateTimeOffset GetUtcNow()
    {
        Calls++;

        return now;
    }
}
