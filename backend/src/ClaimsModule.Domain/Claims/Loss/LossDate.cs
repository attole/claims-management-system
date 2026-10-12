namespace ClaimsModule.Domain.Claims.Loss;

public sealed record LossDate(DateTimeOffset Value)
{
    public static LossDate Empty { get; } = new(default(DateTimeOffset));

    public bool IsSet => Value != default;
}
