using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.ClaimDocuments.Rules;

internal sealed class IsContentMatchingType(string contentType, IReadOnlyList<byte> header) : IRule<object>
{
    public const string Code = "DOC_CONTENT_MISMATCH";

    private const string Message = "The file content does not match its type.";
    private const string Field = "File";

    public CheckResult Check(object subject)
    {
        bool isMatching = DocumentContentTypes.IsMatching(contentType, header);

        return isMatching
            ? CheckResult.Pass
            : CheckResult.Found(new Finding(Code, Message, Field));
    }
}
