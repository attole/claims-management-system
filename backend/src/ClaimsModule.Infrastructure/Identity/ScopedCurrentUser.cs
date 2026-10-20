using ClaimsModule.Application.Abstractions;
using ClaimsModule.Domain.Roles;

namespace ClaimsModule.Infrastructure.Identity;

public sealed class ScopedCurrentUser : ICurrentUser
{
    public Guid Id { get; private set; } = Guid.Empty;

    public Guid OrganisationId { get; private set; } = Guid.Empty;

    public UserRole Role { get; private set; } = UserRole.From(UserRoleKind.Handler);

    public void Set(Guid id, Guid organisationId, UserRole role)
    {
        Id = id;
        OrganisationId = organisationId;
        Role = role;
    }
}
