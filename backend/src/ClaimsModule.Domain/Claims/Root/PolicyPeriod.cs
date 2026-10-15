namespace ClaimsModule.Domain.Claims.Root;

public sealed record PolicyPeriod(DateOnly StartDate, DateOnly EndDate, DateOnly CancelledDate)
{
    public static PolicyPeriod Empty { get; } = new(DateOnly.MinValue, DateOnly.MinValue, DateOnly.MinValue);

    public bool IsSet => EndDate != DateOnly.MinValue;

    public bool IsCancelled => CancelledDate != DateOnly.MinValue;

    public DateOnly EffectiveEndDate => IsCancelled && CancelledDate < EndDate ? CancelledDate : EndDate;

    public bool CanCover(DateOnly date)
    {
        bool isStarted = date >= StartDate;
        bool isNotEnded = date <= EffectiveEndDate;

        return isStarted && isNotEnded;
    }
}
