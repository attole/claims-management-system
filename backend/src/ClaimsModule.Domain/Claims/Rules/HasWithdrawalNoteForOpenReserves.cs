using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.Claims.Rules;

internal sealed class HasWithdrawalNoteForOpenReserves(bool hasOpenReserveBalance, bool hasNote) : IRule<object>
{
    public const string Code = "CLAIM_WITHDRAW_NOTE_REQUIRED";

    private const string Message = "Claim cannot be withdrawn with an open reserve balance without a note.";
    private const string Field = "Note";

    public CheckResult Check(object subject)
    {
        bool hasWithdrawalNote = !hasOpenReserveBalance || hasNote;

        return hasWithdrawalNote
            ? CheckResult.Pass
            : CheckResult.Found(new Finding(Code, Message, Field));
    }
}
