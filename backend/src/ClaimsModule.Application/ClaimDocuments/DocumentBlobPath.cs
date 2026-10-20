using System.Text.RegularExpressions;

namespace ClaimsModule.Application.ClaimDocuments;

public static partial class DocumentBlobPath
{
    public const string Container = "claim-documents";

    private const string FallbackName = "file";

    public static string Build(Guid organisationId, Guid claimId, Guid uploadId, string fileName)
    {
        return $"{Container}/{organisationId}/{claimId}/{uploadId:N}_{Sanitise(fileName)}";
    }

    public static string Sanitise(string fileName)
    {
        var name = Path.GetFileName(fileName.Replace('\\', '/'));
        var cleaned = UnsafeCharacters().Replace(name, "_").Trim('.');

        return cleaned.Length == 0 ? FallbackName : cleaned;
    }

    [GeneratedRegex("[^A-Za-z0-9._-]")]
    private static partial Regex UnsafeCharacters();
}
