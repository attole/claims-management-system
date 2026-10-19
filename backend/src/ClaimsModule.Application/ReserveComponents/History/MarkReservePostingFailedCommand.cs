using ClaimsModule.Application.Abstractions;
using ClaimsModule.Application.Messaging;
using ClaimsModule.Application.Results;
using ClaimsModule.Domain.ReserveComponents.History.Inputs;
using FluentValidation;
using MediatR;

namespace ClaimsModule.Application.ReserveComponents.History;

public sealed record MarkReservePostingFailedCommand(
    Guid ClaimId,
    Guid ReserveHistoryId,
    string JobId,
    string FailureReason) : ICommand<Result<ReserveHistoryStatusDto>>
{
    public HistoryPostingInput ToInput()
    {
        return new HistoryPostingInput(ReserveHistoryId)
        {
            JobId = JobId,
            FailureReason = FailureReason,
        };
    }
}

public sealed class MarkReservePostingFailedCommandValidator : AbstractValidator<MarkReservePostingFailedCommand>
{
    public MarkReservePostingFailedCommandValidator()
    {
        RuleFor(command => command.ClaimId).NotEmpty();
        RuleFor(command => command.ReserveHistoryId).NotEmpty();
        RuleFor(command => command.JobId).NotNull();
        RuleFor(command => command.FailureReason).NotNull();
    }
}

public sealed class MarkReservePostingFailedCommandHandler(
    IReserveComponentRepository components,
    IUnitOfWork unitOfWork) : IRequestHandler<MarkReservePostingFailedCommand, Result<ReserveHistoryStatusDto>>
{
    public async Task<Result<ReserveHistoryStatusDto>> Handle(
        MarkReservePostingFailedCommand command,
        CancellationToken cancellationToken)
    {
        var loaded = await components.GetByHistoryAsync(command.ClaimId, command.ReserveHistoryId, cancellationToken);

        if (!loaded.IsSuccess)
        {
            return loaded.ToFailure<ReserveHistoryStatusDto>();
        }

        var input = command.ToInput();
        var result = loaded.Value.MarkHistoryPostingFailed(input).ToResult();

        if (result.IsSuccess)
        {
            await unitOfWork.CommitAsync(cancellationToken);
        }

        return result.Map(move => new ReserveHistoryStatusDto(command.ReserveHistoryId, move.From.Name, move.To.Name));
    }
}
