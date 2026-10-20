using ClaimsModule.Domain.Common.Capabilities;

namespace ClaimsModule.Persistence.ClaimNumbers;

internal sealed class ClaimNumberCounter : ITenantOwned
{
    public Guid OrganisationId { get; init; } = Guid.Empty;

    public int Year { get; init; } = 0;

    public int LastValue { get; init; } = 0;
}
