using ClaimsModule.Domain.Core.Base.Checks;
using ClaimsModule.Domain.Core.Operations.Creation;
using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.Claims.Root.Rules;

public sealed class HasClaimNumber(ClaimNumber claimNumber) : IRule<NoSubject>
{
    public const string Code = "CLAIM_NUMBER_REQUIRED";

    private const string Message = "A claim needs a claim number.";
    private const string Field = "ClaimNumber";

    public CheckResult<DomainFinding> Check(NoSubject subject)
    {
        bool hasClaimNumber = !claimNumber.Equals(ClaimNumber.Empty);

        return hasClaimNumber
            ? CheckResult<DomainFinding>.Pass
            : CheckResult<DomainFinding>.Found(new RuleFinding(Code, Message, Field));
    }
}
