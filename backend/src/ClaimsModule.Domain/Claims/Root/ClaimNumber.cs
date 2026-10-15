using System.Globalization;
using System.Text.RegularExpressions;

namespace ClaimsModule.Domain.Claims.Root;

public sealed partial record ClaimNumber
{
    private ClaimNumber(string value)
    {
        Value = value;
    }

    public static ClaimNumber Empty { get; } = new(string.Empty);

    public string Value { get; }

    public static ClaimNumber Create(int year, int sequence)
    {
        string value = string.Create(CultureInfo.InvariantCulture, $"CLM-{year:D4}-{sequence:D7}");

        return From(value);
    }

    public static ClaimNumber From(string value)
    {
        bool isClaimNumber = Pattern().IsMatch(value);

        return isClaimNumber
            ? new ClaimNumber(value)
            : throw new ArgumentException($"'{value}' is not a claim number.", nameof(value));
    }

    [GeneratedRegex(@"\ACLM-[0-9]{4}-[0-9]{7}\z")]
    private static partial Regex Pattern();
}
