using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.Claims.Rules;

internal sealed class HasClosingNoteForOpenReserves(bool hasOpenReserveBalance, bool hasNote) : IRule<object>
{
    public const string Code = "S:CC-04";

    private const string Message = "Claim cannot be closed - a closing note for the open reserves is not satisfied.";
    private const string Field = "Note";

    public CheckResult Check(object subject)
    {
        bool hasClosingNote = !hasOpenReserveBalance || hasNote;

        return hasClosingNote
            ? CheckResult.Pass
            : CheckResult.Found(new Finding(Code, Message, Field));
    }
}
