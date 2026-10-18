namespace ClaimsModule.Domain.Policies;

public sealed record PolicyPeriod(Guid PolicyId, DateOnly StartDate, DateOnly EndDate, DateOnly CancelledDate)
{
    private const string OtherPolicyMessage = "The policy period was built for another policy than the claim's.";

    public static PolicyPeriod Empty { get; } = new(
        Guid.Empty,
        DateOnly.MinValue,
        DateOnly.MinValue,
        DateOnly.MinValue);

    public bool IsSet => EndDate != DateOnly.MinValue;

    public bool IsCancelled => CancelledDate != DateOnly.MinValue;

    public DateOnly EffectiveEndDate => IsCancelled && CancelledDate < EndDate ? CancelledDate : EndDate;

    public bool CanCover(DateOnly date)
    {
        bool isStarted = date >= StartDate;
        bool isNotEnded = date <= EffectiveEndDate;

        return isStarted && isNotEnded;
    }

    public PolicyPeriod RequireFor(Guid policyId)
    {
        bool isForPolicy = PolicyId == policyId;

        return isForPolicy
            ? this
            : throw new InvalidOperationException(OtherPolicyMessage);
    }
}
