using AutoMapper;
using ClaimsModule.Application.Abstractions;
using ClaimsModule.Application.ClaimDocuments.Contracts;
using ClaimsModule.Application.ClaimDocuments.Dtos;
using ClaimsModule.Application.Claims.Contracts;
using ClaimsModule.Application.Messaging;
using ClaimsModule.Application.Results;
using ClaimsModule.Domain.ClaimDocuments.Inputs;
using ClaimsModule.Domain.Claims;
using FluentValidation;
using MediatR;

namespace ClaimsModule.Application.ClaimDocuments.Commands;

public sealed record RemoveDocumentCommand(Guid ClaimId, Guid DocumentId) : ICommand<Result<ClaimDocumentDto>>
{
    public RemoveClaimDocumentInput ToInput(ClaimState claimState)
    {
        return new RemoveClaimDocumentInput(claimState);
    }
}

public sealed class RemoveDocumentCommandValidator : AbstractValidator<RemoveDocumentCommand>
{
    public RemoveDocumentCommandValidator()
    {
        RuleFor(command => command.ClaimId).NotEmpty();
        RuleFor(command => command.DocumentId).NotEmpty();
    }
}

public sealed class RemoveDocumentCommandHandler(
    IClaimRepository claims,
    IClaimDocumentRepository documents,
    IUnitOfWork unitOfWork,
    IMapper mapper) : IRequestHandler<RemoveDocumentCommand, Result<ClaimDocumentDto>>
{
    public async Task<Result<ClaimDocumentDto>> Handle(
        RemoveDocumentCommand command,
        CancellationToken cancellationToken)
    {
        var loadedClaim = await claims.GetAsync(command.ClaimId, cancellationToken);

        if (!loadedClaim.IsSuccess)
        {
            return loadedClaim.ToFailure<ClaimDocumentDto>();
        }

        var loaded = await documents.GetAsync(command.ClaimId, command.DocumentId, cancellationToken);

        if (!loaded.IsSuccess)
        {
            return loaded.ToFailure<ClaimDocumentDto>();
        }

        var input = command.ToInput(loadedClaim.Value.State);
        var result = loaded.Value.Remove(input).ToResult();

        if (result.IsSuccess)
        {
            await unitOfWork.CommitAsync(cancellationToken);
        }

        return result.Map(document => mapper.Map<ClaimDocumentDto>(document));
    }
}
