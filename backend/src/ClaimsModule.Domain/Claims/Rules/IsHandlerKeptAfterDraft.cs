using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.Claims.Rules;

internal sealed class IsHandlerKeptAfterDraft(Guid handlerId) : IRule<Claim>
{
    public const string Code = "CLAIM_HANDLER_NOT_KEPT";

    private const string Message = "An open claim cannot be left without a handler.";
    private const string Field = "HandlerId";

    public CheckResult Check(Claim claim)
    {
        bool isHandlerKept = handlerId != Guid.Empty || claim.State.IsDraft;

        return isHandlerKept
            ? CheckResult.Pass
            : CheckResult.Found(new Finding(Code, Message, Field));
    }
}
