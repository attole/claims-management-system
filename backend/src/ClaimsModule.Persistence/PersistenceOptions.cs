using System.ComponentModel.DataAnnotations;

namespace ClaimsModule.Persistence;

public sealed class PersistenceOptions
{
    public const string SectionName = "ConnectionStrings";

    [Required]
    public string ClaimsDb { get; init; } = string.Empty;
}
