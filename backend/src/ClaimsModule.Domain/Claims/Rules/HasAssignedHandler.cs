using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.Claims.Rules;

internal sealed class HasAssignedHandler : IRule<Claim>
{
    public const string Code = "CLAIM_HANDLER_REQUIRED";

    private const string Message = "A claim needs an assigned handler to open.";
    private const string Field = "AssignedHandlerId";

    public CheckResult Check(Claim claim)
    {
        bool hasAssignedHandler = claim.HasAssignedHandler;

        return hasAssignedHandler
            ? CheckResult.Pass
            : CheckResult.Found(new Finding(Code, Message, Field));
    }
}
