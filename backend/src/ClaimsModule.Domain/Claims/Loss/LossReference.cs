namespace ClaimsModule.Domain.Claims.Loss;

public sealed record LossReference(IReadOnlyCollection<string> ActiveCauseCodes, PolicyPeriod PolicyPeriod)
{
    private const string MissingCodesMessage = "A loss reference needs the active cause of loss codes.";
    private const string OtherPolicyMessage = "The loss reference was built for another policy than the claim's.";

    public IReadOnlyCollection<string> ActiveCauseCodes { get; } = RequiredCodes(ActiveCauseCodes);

    public bool IsActiveCauseCode(string code)
    {
        bool isActive = ActiveCauseCodes.Any(active => string.Equals(active, code, StringComparison.OrdinalIgnoreCase));

        return isActive;
    }

    public LossReference RequireFor(Guid policyId)
    {
        bool isForPolicy = PolicyPeriod.PolicyId == policyId;

        return isForPolicy
            ? this
            : throw new InvalidOperationException(OtherPolicyMessage);
    }

    private static IReadOnlyCollection<string> RequiredCodes(IReadOnlyCollection<string> codes)
    {
        bool hasCodes = codes.Count > 0;

        return hasCodes
            ? codes
            : throw new ArgumentException(MissingCodesMessage, nameof(codes));
    }
}
