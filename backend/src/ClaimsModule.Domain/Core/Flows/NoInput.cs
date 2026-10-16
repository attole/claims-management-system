namespace ClaimsModule.Domain.Core.Flows;

public sealed class NoInput
{
    private NoInput()
    {
    }

    public static NoInput Value { get; } = new();
}
