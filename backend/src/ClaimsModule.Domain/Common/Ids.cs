namespace ClaimsModule.Domain.Common;

public static class Ids
{
    private const int TimestampLength = 6;
    private const int TimestampOffset = 10;

    public static Guid Next(DateTimeOffset now)
    {
        byte[] bytes = Guid.NewGuid().ToByteArray();
        ulong milliseconds = (ulong)now.ToUnixTimeMilliseconds();

        for (int index = 0; index < TimestampLength; index++)
        {
            int shift = 8 * (TimestampLength - 1 - index);
            bytes[TimestampOffset + index] = (byte)(milliseconds >> shift);
        }

        return new Guid(bytes);
    }
}
