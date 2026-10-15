namespace ClaimsModule.Domain.Claims.Loss;

public sealed record EstimatedLossAmount
{
    private EstimatedLossAmount(decimal value, bool isSet)
    {
        Value = value;
        IsSet = isSet;
    }

    public EstimatedLossAmount(decimal value) : this(Validated(value), true)
    {
    }

    public static EstimatedLossAmount Empty { get; } = new(0m, false);

    public decimal Value { get; }

    public bool IsSet { get; }

    private static decimal Validated(decimal value)
    {
        return value >= 0m
            ? value
            : throw new ArgumentOutOfRangeException(nameof(value), "An estimated loss cannot be negative.");
    }
}
