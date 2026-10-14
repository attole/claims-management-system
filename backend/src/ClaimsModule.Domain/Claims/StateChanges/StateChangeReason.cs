namespace ClaimsModule.Domain.Claims.StateChanges;

public sealed record StateChangeReason(string Code, string Text)
{
    public static StateChangeReason Empty { get; } = new(string.Empty, string.Empty);

    public bool IsSet => !string.IsNullOrWhiteSpace(Code) || !string.IsNullOrWhiteSpace(Text);

    public bool IsComplete => !string.IsNullOrWhiteSpace(Code) && !string.IsNullOrWhiteSpace(Text);
}
