using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.ClaimDocuments.Rules;

internal sealed class IsContentTypeAllowed(string contentType) : IRule<object>
{
    public const string Code = "S:DOC-MimeAllowlist";

    private const string Message = "The file type is not allowed.";
    private const string Field = "ContentType";

    private static readonly HashSet<string> _allowed = new(StringComparer.OrdinalIgnoreCase)
    {
        "application/pdf",
        "image/jpeg",
        "image/png",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        "text/plain",
        "text/csv",
    };

    public CheckResult Check(object subject)
    {
        bool isAllowed = _allowed.Contains(contentType);

        return isAllowed
            ? CheckResult.Pass
            : CheckResult.Found(new Finding(Code, Message, Field));
    }
}
