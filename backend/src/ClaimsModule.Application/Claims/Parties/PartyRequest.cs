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

    public PartyDetails ToDetails()
    {
        var role = PartyRole.From(Role);
        var type = PartyType.From(Type);

        return new PartyDetails(role, type)
        {
            FirstName = FirstName ?? string.Empty,
            LastName = LastName ?? string.Empty,
            CompanyName = CompanyName ?? string.Empty,
            Email = Email ?? string.Empty,
            Phone = Phone ?? string.Empty,
            Notes = Notes ?? string.Empty,
        };
    }
}
