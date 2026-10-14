using ClaimsModule.Domain.Common.Markers;

namespace ClaimsModule.Domain.Common.Roles;

public enum UserRoleKind
{
    Handler,
    Supervisor,
    Manager,
}

public abstract record UserRole(UserRoleKind Marker) : Marked<UserRoleKind>(Marker);

public sealed record UserHandlerRole() : UserRole(UserRoleKind.Handler);

public sealed record UserSupervisorRole() : UserRole(UserRoleKind.Supervisor);

public sealed record UserManagerRole() : UserRole(UserRoleKind.Manager);
