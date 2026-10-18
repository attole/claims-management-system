namespace ClaimsModule.Domain.Claims.Loss;

public sealed record CauseCodes(IReadOnlyCollection<string> Active)
{
    private const string MissingCodesMessage = "The cause codes need the active cause of loss codes.";

    public IReadOnlyCollection<string> Active { get; } = RequiredCodes(Active);

    public bool IsActive(string code)
    {
        bool isActive = Active.Any(active => string.Equals(active, code, StringComparison.OrdinalIgnoreCase));

        return isActive;
    }

    private static IReadOnlyCollection<string> RequiredCodes(IReadOnlyCollection<string> codes)
    {
        bool hasCodes = codes.Count > 0;

        return hasCodes
            ? codes
            : throw new ArgumentException(MissingCodesMessage, nameof(codes));
    }
}
