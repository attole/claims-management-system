using ClaimsModule.Domain.Claims.Parties;

namespace ClaimsModule.Application.Claims.Parties;

public sealed record PartyRequest(PartyRoleKind Role, PartyTypeKind Type)
{
    public string FirstName { get; init; } = string.Empty;

    public string LastName { get; init; } = string.Empty;

    public string CompanyName { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;

    public string Phone { get; init; } = string.Empty;

    public string Notes { get; init; } = string.Empty;
}
