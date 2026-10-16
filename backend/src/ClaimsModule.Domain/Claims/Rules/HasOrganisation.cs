using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.Claims.Rules;

internal sealed class HasOrganisation(Guid organisationId) : IRule<object>
{
    public const string Code = "CLAIM_ORGANISATION_REQUIRED";

    private const string Message = "A claim needs an organisation.";
    private const string Field = "OrganisationId";

    public CheckResult Check(object subject)
    {
        bool hasOrganisation = organisationId != Guid.Empty;

        return hasOrganisation
            ? CheckResult.Pass
            : CheckResult.Found(new Finding(Code, Message, Field));
    }
}
