namespace ClaimsModule.Application.Claims.Parties;

public sealed record PartyDto
{
    public Guid Id { get; init; } = Guid.Empty;

    public Guid ClaimId { get; init; } = Guid.Empty;

    public string Role { get; init; } = string.Empty;

    public string Type { get; init; } = string.Empty;

    public string FirstName { get; init; } = string.Empty;

    public string LastName { get; init; } = string.Empty;

    public string CompanyName { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;

    public string Phone { get; init; } = string.Empty;

    public string Notes { get; init; } = string.Empty;

    public bool IsActive { get; init; } = false;
}
