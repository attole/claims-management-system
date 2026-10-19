using ClaimsModule.Application.Abstractions;
using ClaimsModule.Application.Messaging;
using ClaimsModule.Application.Results;
using ClaimsModule.Domain.ReserveComponents.History.Inputs;
using FluentValidation;
using MediatR;

namespace ClaimsModule.Application.ReserveComponents.History;

public sealed record RetryReservePostingCommand(Guid ClaimId, Guid ReserveHistoryId)
    : ICommand<Result<ReserveHistoryStatusDto>>
{
    public HistoryPostingInput ToInput(string jobId)
    {
        return new HistoryPostingInput(ReserveHistoryId)
        {
            JobId = jobId,
        };
    }
}

public sealed class RetryReservePostingCommandValidator : AbstractValidator<RetryReservePostingCommand>
{
    public RetryReservePostingCommandValidator()
    {
        RuleFor(command => command.ClaimId).NotEmpty();
        RuleFor(command => command.ReserveHistoryId).NotEmpty();
    }
}

public sealed class RetryReservePostingCommandHandler(
    IReserveComponentRepository components,
    IUnitOfWork unitOfWork) : IRequestHandler<RetryReservePostingCommand, Result<ReserveHistoryStatusDto>>
{
    public async Task<Result<ReserveHistoryStatusDto>> Handle(
        RetryReservePostingCommand command,
        CancellationToken cancellationToken)
    {
        var loaded = await components.GetByHistoryAsync(command.ClaimId, command.ReserveHistoryId, cancellationToken);

        if (!loaded.IsSuccess)
        {
            return loaded.ToFailure<ReserveHistoryStatusDto>();
        }

        var input = command.ToInput(Guid.NewGuid().ToString("N"));
        var result = loaded.Value.RetryHistoryPosting(input).ToResult();

        if (result.IsSuccess)
        {
            await unitOfWork.CommitAsync(cancellationToken);
        }

        return result.Map(move => new ReserveHistoryStatusDto(command.ReserveHistoryId, move.From.Name, move.To.Name));
    }
}
