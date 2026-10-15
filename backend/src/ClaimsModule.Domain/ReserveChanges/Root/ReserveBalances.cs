namespace ClaimsModule.Domain.ReserveChanges.Root;

public sealed record ReserveBalances
{
    private ReserveBalances(decimal previousBalance, decimal newBalance, bool isSet)
    {
        PreviousBalance = previousBalance;
        NewBalance = newBalance;
        IsSet = isSet;
    }

    public ReserveBalances(decimal previousBalance, decimal newBalance)
        : this(previousBalance, newBalance, true)
    {
    }

    public static ReserveBalances Empty { get; } = new(0m, 0m, false);

    public decimal PreviousBalance { get; }

    public decimal NewBalance { get; }

    public bool IsSet { get; }
}
