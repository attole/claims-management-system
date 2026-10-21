using System.Collections.Frozen;
using ClaimsModule.Domain.Roles;

namespace ClaimsModule.API.Identity;

public static class RoleNames
{
    private static readonly FrozenDictionary<string, UserRoleKind> _kinds = Enum.GetValues<UserRoleKind>()
        .ToFrozenDictionary(kind => kind.ToString(), StringComparer.Ordinal);

    public static bool IsKnown(string? value, out UserRoleKind kind)
    {
        return _kinds.TryGetValue(value ?? string.Empty, out kind);
    }
}
