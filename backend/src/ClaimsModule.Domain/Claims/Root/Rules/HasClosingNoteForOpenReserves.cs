using ClaimsModule.Domain.Core.Base.Checks;
using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.Claims.Root.Rules;

internal sealed class HasClosingNoteForOpenReserves(ClaimReserveSnapshot reserves, string note) : IRule<Claim>
{
    public const string Code = "S:CC-04";

    private const string Message = "Claim cannot be closed - a closing note for the open reserves is not satisfied.";
    private const string Field = "Note";

    public CheckResult<DomainFinding> Check(Claim claim)
    {
        bool hasClosingNote = !reserves.HasOpenReserveBalance || !string.IsNullOrWhiteSpace(note);

        return hasClosingNote
            ? CheckResult<DomainFinding>.Pass
            : CheckResult<DomainFinding>.Found(new RuleFinding(Code, Message, Field));
    }
}
