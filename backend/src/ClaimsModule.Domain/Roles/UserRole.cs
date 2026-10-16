using ClaimsModule.Domain.Common.Markers;

namespace ClaimsModule.Domain.Roles;

public enum UserRoleKind
{
    Handler,
    Supervisor,
    Manager,
}

public abstract record UserRole(UserRoleKind Marker) : Marked<UserRoleKind>(Marker)
{
    protected abstract IReadOnlyCollection<UserRoleKind> HeldRoles { get; }

    public static UserRole From(UserRoleKind kind)
    {
        return kind switch
        {
            UserRoleKind.Handler => new UserHandlerRole(),
            UserRoleKind.Supervisor => new UserSupervisorRole(),
            UserRoleKind.Manager => new UserManagerRole(),
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "Unknown role."),
        };
    }

    public bool HasRightsOf(UserRole role)
    {
        bool holdsRole = HeldRoles.Contains(role.Marker);

        return holdsRole;
    }

    public virtual bool CanDecideReserve(decimal amount)
    {
        return false;
    }
}

public sealed record UserHandlerRole() : UserRole(UserRoleKind.Handler)
{
    protected override IReadOnlyCollection<UserRoleKind> HeldRoles => [UserRoleKind.Handler];
}

public sealed record UserSupervisorRole() : UserRole(UserRoleKind.Supervisor)
{
    private const decimal ReserveDecisionLimit = 100_000m;

    protected override IReadOnlyCollection<UserRoleKind> HeldRoles => [UserRoleKind.Handler, UserRoleKind.Supervisor];

    public override bool CanDecideReserve(decimal amount)
    {
        bool isWithinLimit = amount <= ReserveDecisionLimit;

        return isWithinLimit;
    }
}

public sealed record UserManagerRole() : UserRole(UserRoleKind.Manager)
{
    protected override IReadOnlyCollection<UserRoleKind> HeldRoles =>
        [UserRoleKind.Handler, UserRoleKind.Supervisor, UserRoleKind.Manager];

    public override bool CanDecideReserve(decimal amount)
    {
        return true;
    }
}
