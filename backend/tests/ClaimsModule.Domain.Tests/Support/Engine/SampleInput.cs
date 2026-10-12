using ClaimsModule.Domain.Engine.Operations;

namespace ClaimsModule.Domain.Tests.Support.Engine;

public sealed record SampleInput(string Text, DateTimeOffset Now) : OperationInput(Now)
{
    public static readonly DateTimeOffset Instant = new(2026, 10, 12, 8, 0, 0, TimeSpan.Zero);

    public SampleInput(string text)
        : this(text, Instant)
    {
    }
}
