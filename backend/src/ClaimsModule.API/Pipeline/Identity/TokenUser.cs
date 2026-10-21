using System.Security.Claims;
using ClaimsModule.Domain.Roles;

namespace ClaimsModule.API.Pipeline.Identity;

public readonly record struct TokenUser(Guid Id, Guid OrganisationId, UserRole Role)
{
    public static readonly TokenUser Empty = new(Guid.Empty, Guid.Empty, UserRole.From(UserRoleKind.Handler));

    public static bool CanRead(ClaimsPrincipal principal, out TokenUser user)
    {
        var subject = principal.FindFirstValue(IdentityClaims.Subject);
        var organisation = principal.FindFirstValue(IdentityClaims.Organisation);
        bool hasId = Guid.TryParse(subject, out var id);
        bool hasOrganisation = Guid.TryParse(organisation, out var organisationId);
        bool hasRole = RoleNames.IsKnown(principal.FindFirstValue(IdentityClaims.Role), out var kind);
        bool isComplete = hasId && hasOrganisation && hasRole && id != Guid.Empty && organisationId != Guid.Empty;

        user = isComplete ? new TokenUser(id, organisationId, UserRole.From(kind)) : Empty;

        return isComplete;
    }
}
