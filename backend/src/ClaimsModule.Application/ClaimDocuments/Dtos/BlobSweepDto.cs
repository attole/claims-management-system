namespace ClaimsModule.Application.ClaimDocuments.Dtos;

public sealed record BlobSweepDto
{
    public int DeletedCount { get; init; } = 0;
}
