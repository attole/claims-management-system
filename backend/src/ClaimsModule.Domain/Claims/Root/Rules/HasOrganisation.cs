using ClaimsModule.Domain.Core.Base.Checks;
using ClaimsModule.Domain.Core.Operations.Creation;
using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.Claims.Root.Rules;

public sealed class HasOrganisation(Guid organisationId) : IRule<NoSubject>
{
    public const string Code = "CLAIM_ORGANISATION_REQUIRED";

    private const string Message = "A claim needs an organisation.";
    private const string Field = "OrganisationId";

    public CheckResult<DomainFinding> Check(NoSubject subject)
    {
        bool hasOrganisation = organisationId != Guid.Empty;

        return hasOrganisation
            ? CheckResult<DomainFinding>.Pass
            : CheckResult<DomainFinding>.Found(new RuleFinding(Code, Message, Field));
    }
}
