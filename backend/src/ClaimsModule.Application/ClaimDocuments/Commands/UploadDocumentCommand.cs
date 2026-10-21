using AutoMapper;
using ClaimsModule.Application.Abstractions;
using ClaimsModule.Application.ClaimDocuments.Contracts;
using ClaimsModule.Application.ClaimDocuments.Dtos;
using ClaimsModule.Application.Claims.Contracts;
using ClaimsModule.Application.Messaging;
using ClaimsModule.Application.Results;
using ClaimsModule.Domain.ClaimDocuments;
using ClaimsModule.Domain.ClaimDocuments.Inputs;
using ClaimsModule.Domain.Claims;
using FluentValidation;
using MediatR;

namespace ClaimsModule.Application.ClaimDocuments.Commands;

public sealed record UploadDocumentCommand(
    Guid ClaimId,
    string DocumentType,
    string FileName,
    string ContentType,
    long FileSizeBytes) : ICommand<Result<ClaimDocumentDto>>
{
    public Stream Content { get; init; } = Stream.Null;

    public string Notes { get; init; } = string.Empty;

    public UploadClaimDocumentInput ToInput(
        ClaimReference claim,
        string blobPath,
        IReadOnlyList<byte> header,
        Guid actorId,
        DateTimeOffset now)
    {
        var details = new ClaimDocumentDetails(
            DocumentType ?? string.Empty,
            FileName,
            blobPath,
            ContentType ?? string.Empty,
            FileSizeBytes)
        {
            Notes = Notes ?? string.Empty,
        };

        return new UploadClaimDocumentInput(claim, details, actorId, now) { Header = header };
    }
}

public sealed class UploadDocumentCommandValidator : AbstractValidator<UploadDocumentCommand>
{
    public UploadDocumentCommandValidator()
    {
        RuleFor(command => command.ClaimId).NotEmpty();
        RuleFor(command => command.DocumentType).NotNull().MaximumLength(DocumentFieldLengths.DocumentType);
        RuleFor(command => command.FileName).NotEmpty().MaximumLength(DocumentFieldLengths.DocumentName);
        RuleFor(command => command.ContentType).NotNull().MaximumLength(DocumentFieldLengths.ContentType);
        RuleFor(command => command.FileSizeBytes).GreaterThanOrEqualTo(0);
        RuleFor(command => command.Content).NotNull();
        RuleFor(command => command.Notes).NotNull().MaximumLength(DocumentFieldLengths.Notes);
    }
}

public sealed class UploadDocumentCommandHandler(
    IClaimRepository claims,
    IClaimDocumentRepository documents,
    IStorageService storage,
    IUnitOfWork unitOfWork,
    IClock clock,
    ICurrentUser user,
    IMapper mapper) : IRequestHandler<UploadDocumentCommand, Result<ClaimDocumentDto>>
{
    public async Task<Result<ClaimDocumentDto>> Handle(
        UploadDocumentCommand command,
        CancellationToken cancellationToken)
    {
        var loaded = await claims.GetAsync(command.ClaimId, cancellationToken);

        if (!loaded.IsSuccess)
        {
            return loaded.ToFailure<ClaimDocumentDto>();
        }

        var claim = loaded.Value;
        var content = await DocumentContent.ReadAsync(command.Content, cancellationToken);
        var blobPath = DocumentBlobPath.Build(claim.OrganisationId, claim.Id, Guid.NewGuid(), command.FileName);
        var input = command.ToInput(claim.Reference, blobPath, content.Header, user.Id, clock.UtcNow);
        var result = ClaimDocument.Upload(input).ToResult();

        if (!result.IsSuccess)
        {
            return result.ToFailure<ClaimDocumentDto>();
        }

        await storage.UploadAsync(blobPath, content.Content, command.ContentType, cancellationToken);
        await documents.AddAsync(result.Value, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        return result.Map(document => mapper.Map<ClaimDocumentDto>(document));
    }
}
