using ClaimsModule.Application.Abstractions;
using ClaimsModule.Application.ClaimDocuments.Contracts;
using ClaimsModule.Application.ClaimDocuments.Dtos;
using ClaimsModule.Application.Messaging;
using ClaimsModule.Application.Results;
using FluentValidation;
using MediatR;

namespace ClaimsModule.Application.ClaimDocuments.Commands;

public sealed record SweepOrphanBlobsCommand(TimeSpan MinimumAge) : ICommand<Result<BlobSweepDto>>;

public sealed class SweepOrphanBlobsCommandValidator : AbstractValidator<SweepOrphanBlobsCommand>
{
    public SweepOrphanBlobsCommandValidator()
    {
        RuleFor(command => command.MinimumAge).GreaterThan(TimeSpan.Zero);
    }
}

public sealed class SweepOrphanBlobsCommandHandler(
    IStorageService storage,
    IClaimDocumentReader reader,
    IClock clock) : IRequestHandler<SweepOrphanBlobsCommand, Result<BlobSweepDto>>
{
    private const int BatchSize = 500;

    public async Task<Result<BlobSweepDto>> Handle(SweepOrphanBlobsCommand command, CancellationToken cancellationToken)
    {
        var createdBefore = clock.UtcNow - command.MinimumAge;
        var candidates = await storage.ListAsync(DocumentBlobPath.Container, createdBefore, cancellationToken);
        int deleted = 0;

        foreach (var batch in candidates.Chunk(BatchSize))
        {
            var referenced = await reader.FindReferencedPathsAsync(batch, cancellationToken);
            var orphans = batch.Except(referenced, StringComparer.OrdinalIgnoreCase).ToList();

            foreach (var path in orphans)
            {
                await storage.DeleteAsync(path, cancellationToken);
            }

            deleted += orphans.Count;
        }

        return Result<BlobSweepDto>.Success(new BlobSweepDto { DeletedCount = deleted });
    }
}
