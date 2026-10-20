using ClaimsModule.Domain.Policies;

namespace ClaimsModule.Domain.Claims.Loss;

public sealed record LossDate(DateTimeOffset Value)
{
    public static LossDate Empty { get; } = new(default(DateTimeOffset));

    public bool IsSet => Value != default;

    public DateOnly UtcDay => DateOnly.FromDateTime(Value.UtcDateTime);

    public bool IsAfter(DateTimeOffset time)
    {
        bool isAfter = IsSet && Value > time;

        return isAfter;
    }

    public bool IsCoveredBy(PolicyPeriod period)
    {
        bool isCovered = !IsSet || !period.IsSet || period.CanCover(UtcDay);

        return isCovered;
    }
}
