using ClaimsModule.Domain.Core.Operations;

namespace ClaimsModule.Domain.Tests.Support.Core;

public sealed record SampleInput(string Text, DateTimeOffset Now, Guid ActorId) : OperationInput(Now, ActorId)
{
    public static readonly DateTimeOffset Instant = new(2026, 10, 12, 8, 0, 0, TimeSpan.Zero);
    public static readonly Guid Actor = Guid.Parse("99999999-9999-9999-9999-999999999999");

    public SampleInput(string text) : this(text, Instant, Actor)
    {
    }

    public SampleInput(string text, DateTimeOffset now) : this(text, now, Actor)
    {
    }
}
