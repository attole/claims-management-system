using ClaimsModule.Domain.Claims.Root;

namespace ClaimsModule.Domain.Claims.Issues;

public sealed record ClaimReferenceSnapshot(IReadOnlyCollection<string> ActiveCauseCodes, PolicyPeriod PolicyPeriod);
