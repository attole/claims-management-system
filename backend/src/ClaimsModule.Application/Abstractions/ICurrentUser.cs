using ClaimsModule.Domain.Roles;

namespace ClaimsModule.Application.Abstractions;

public interface ICurrentUser
{
    Guid Id { get; }

    Guid OrganisationId { get; }

    UserRole Role { get; }
}
