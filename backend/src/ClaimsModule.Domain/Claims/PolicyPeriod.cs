using ClaimsModule.Domain.Claims.Loss;

namespace ClaimsModule.Domain.Claims;

public sealed record PolicyPeriod(Guid PolicyId, DateOnly StartDate, DateOnly EndDate, DateOnly CancelledDate)
{
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

    public bool CanCoverLoss(LossDate lossDate)
    {
        bool canCover = !lossDate.IsSet || !IsSet || CanCover(lossDate.UtcDay);

        return canCover;
    }
}
