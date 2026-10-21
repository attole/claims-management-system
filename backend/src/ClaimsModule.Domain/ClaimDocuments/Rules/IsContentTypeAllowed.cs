using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.ClaimDocuments.Rules;

internal sealed class IsContentTypeAllowed(string contentType) : IRule<object>
{
    public const string Code = "S:DOC-MimeAllowlist";

    private const string Message = "The file type is not allowed.";
    private const string Field = "ContentType";

    public CheckResult Check(object subject)
    {
        bool isAllowed = DocumentContentTypes.IsAllowed(contentType);

        return isAllowed
            ? CheckResult.Pass
            : CheckResult.Found(new Finding(Code, Message, Field));
    }
}
