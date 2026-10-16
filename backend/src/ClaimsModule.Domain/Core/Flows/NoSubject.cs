namespace ClaimsModule.Domain.Core.Flows;

public sealed class NoSubject
{
    private NoSubject()
    {
    }

    public static NoSubject Value { get; } = new();
}
