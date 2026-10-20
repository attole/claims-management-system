using AutoMapper;
using ClaimsModule.Application.ClaimDocuments.Contracts;
using ClaimsModule.Application.ClaimDocuments.Dtos;
using ClaimsModule.Domain.ClaimDocuments;
using Microsoft.EntityFrameworkCore;

namespace ClaimsModule.Persistence.Readers;

internal sealed class ClaimDocumentReader(ClaimsDbContext context, IMapper mapper) : IClaimDocumentReader
{
    public async Task<IReadOnlyList<ClaimDocumentFile>> ListAsync(Guid claimId, CancellationToken cancellationToken)
    {
        var documents = await context.ClaimDocuments
            .AsNoTracking()
            .Where(document => document.ClaimId == claimId)
            .ToListAsync(cancellationToken);

        var files = documents
            .OrderBy(document => document.UploadedAt)
            .Select(document => ToFile(document))
            .ToList();

        return files;
    }

    private ClaimDocumentFile ToFile(ClaimDocument document)
    {
        var dto = mapper.Map<ClaimDocumentDto>(document);
        var file = new ClaimDocumentFile(dto, document.Details.BlobPath);

        return file;
    }
}
