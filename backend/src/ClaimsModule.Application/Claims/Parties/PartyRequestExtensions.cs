using ClaimsModule.Domain.Claims.Parties;

namespace ClaimsModule.Application.Claims.Parties;

public static class PartyRequestExtensions
{
    public static PartyDetails ToDetails(this PartyRequest request)
    {
        var role = PartyRole.From(request.Role);
        var type = PartyType.From(request.Type);

        return new PartyDetails(role, type)
        {
            FirstName = request.FirstName ?? string.Empty,
            LastName = request.LastName ?? string.Empty,
            CompanyName = request.CompanyName ?? string.Empty,
            Email = request.Email ?? string.Empty,
            Phone = request.Phone ?? string.Empty,
            Notes = request.Notes ?? string.Empty,
        };
    }
}
