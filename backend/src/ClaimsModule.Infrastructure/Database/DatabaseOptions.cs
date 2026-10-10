using System.ComponentModel.DataAnnotations;

namespace ClaimsModule.Infrastructure.Database;

public sealed class DatabaseOptions
{
    public const string SectionName = "ConnectionStrings";

    [Required]
    public string ClaimsDb { get; init; } = string.Empty;
}
