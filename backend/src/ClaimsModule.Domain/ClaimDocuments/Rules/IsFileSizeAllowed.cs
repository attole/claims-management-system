using ClaimsModule.Domain.Core.Rules;

namespace ClaimsModule.Domain.ClaimDocuments.Rules;

internal sealed class IsFileSizeAllowed(long fileSizeBytes) : IRule<object>
{
    public const string Code = "S:DOC-MaxSize";
    public const long MaxBytes = 50L * 1024 * 1024;

    private const string Message = "The file is larger than the 50 MB limit.";
    private const string Field = "FileSizeBytes";

    public CheckResult Check(object subject)
    {
        bool isAllowed = fileSizeBytes <= MaxBytes;

        return isAllowed
            ? CheckResult.Pass
            : CheckResult.Found(new Finding(Code, Message, Field));
    }
}
