namespace ClaimsModule.Domain.Claims.Loss;

public sealed record EstimatedLossAmount
{
    private EstimatedLossAmount(decimal value, bool isSet)
    {
        Value = value;
        IsSet = isSet;
    }

    public EstimatedLossAmount(decimal value)
        : this(value, true)
    {
    }

    public static EstimatedLossAmount Empty { get; } = new(0m, false);

    public decimal Value { get; }

    public bool IsSet { get; }
}
