using ClaimsModule.Application.Abstractions;
using ClaimsModule.Application.Messaging;
using ClaimsModule.Application.Results;
using ClaimsModule.Domain.ReserveComponents.History.Inputs;
using FluentValidation;
using MediatR;

namespace ClaimsModule.Application.ReserveComponents.History;

public sealed record RetractReserveCommand(Guid ClaimId, Guid ReserveHistoryId)
    : ICommand<Result<ReserveHistoryStatusDto>>
{
    public string Reason { get; init; } = string.Empty;

    public RetractHistoryInput ToInput(Guid actorId, DateTimeOffset now)
    {
        return new RetractHistoryInput(ReserveHistoryId, actorId, now)
        {
            Reason = Reason ?? string.Empty,
        };
    }
}

public sealed class RetractReserveCommandValidator : AbstractValidator<RetractReserveCommand>
{
    public RetractReserveCommandValidator()
    {
        RuleFor(command => command.ClaimId).NotEmpty();
        RuleFor(command => command.ReserveHistoryId).NotEmpty();
    }
}

public sealed class RetractReserveCommandHandler(
    IReserveComponentRepository components,
    IUnitOfWork unitOfWork,
    IClock clock,
    ICurrentUser user) : IRequestHandler<RetractReserveCommand, Result<ReserveHistoryStatusDto>>
{
    public async Task<Result<ReserveHistoryStatusDto>> Handle(
        RetractReserveCommand command,
        CancellationToken cancellationToken)
    {
        var loaded = await components.GetByHistoryAsync(command.ClaimId, command.ReserveHistoryId, cancellationToken);

        if (!loaded.IsSuccess)
        {
            return loaded.ToFailure<ReserveHistoryStatusDto>();
        }

        var input = command.ToInput(user.Id, clock.UtcNow);
        var result = loaded.Value.RetractHistory(input).ToResult();

        if (result.IsSuccess)
        {
            await unitOfWork.CommitAsync(cancellationToken);
        }

        return result.Map(move => new ReserveHistoryStatusDto(command.ReserveHistoryId, move.From.Name, move.To.Name));
    }
}
