namespace ClaimsModule.Domain.Common;

internal static class InputTime
{
    private const string MissingTimeMessage = "An input needs the time of the change.";
    private const string EarlyTimeMessage = "A business time cannot be earlier than the time it follows.";

    public static DateTimeOffset Required(DateTimeOffset now)
    {
        bool isSet = now != DateTimeOffset.MinValue;

        return isSet
            ? now
            : throw new ArgumentException(MissingTimeMessage, nameof(now));
    }

    public static DateTimeOffset NotBefore(DateTimeOffset time, DateTimeOffset earliest)
    {
        bool isInOrder = time >= earliest;

        return isInOrder
            ? time
            : throw new ArgumentOutOfRangeException(nameof(time), EarlyTimeMessage);
    }
}
