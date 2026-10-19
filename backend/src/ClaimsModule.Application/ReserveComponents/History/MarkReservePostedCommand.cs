using ClaimsModule.Application.Abstractions;
using ClaimsModule.Application.Messaging;
using ClaimsModule.Application.Results;
using ClaimsModule.Domain.ReserveComponents.History.Inputs;
using FluentValidation;
using MediatR;

namespace ClaimsModule.Application.ReserveComponents.History;

public sealed record MarkReservePostedCommand(Guid ClaimId, Guid ReserveHistoryId, string JobId)
    : ICommand<Result<ReserveHistoryStatusDto>>
{
    public HistoryPostingInput ToInput()
    {
        return new HistoryPostingInput(ReserveHistoryId)
        {
            JobId = JobId,
        };
    }
}

public sealed class MarkReservePostedCommandValidator : AbstractValidator<MarkReservePostedCommand>
{
    public MarkReservePostedCommandValidator()
    {
        RuleFor(command => command.ClaimId).NotEmpty();
        RuleFor(command => command.ReserveHistoryId).NotEmpty();
        RuleFor(command => command.JobId).NotNull();
    }
}

public sealed class MarkReservePostedCommandHandler(
    IReserveComponentRepository components,
    IUnitOfWork unitOfWork) : IRequestHandler<MarkReservePostedCommand, Result<ReserveHistoryStatusDto>>
{
    public async Task<Result<ReserveHistoryStatusDto>> Handle(
        MarkReservePostedCommand command,
        CancellationToken cancellationToken)
    {
        var loaded = await components.GetByHistoryAsync(command.ClaimId, command.ReserveHistoryId, cancellationToken);

        if (!loaded.IsSuccess)
        {
            return loaded.ToFailure<ReserveHistoryStatusDto>();
        }

        var input = command.ToInput();
        var result = loaded.Value.MarkHistoryPosted(input).ToResult();

        if (result.IsSuccess)
        {
            await unitOfWork.CommitAsync(cancellationToken);
        }

        return result.Map(move => new ReserveHistoryStatusDto(command.ReserveHistoryId, move.From.Name, move.To.Name));
    }
}
