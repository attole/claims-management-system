using ClaimsModule.Application.Abstractions;
using ClaimsModule.Application.Messaging;
using ClaimsModule.Application.ReserveComponents.Contracts;
using ClaimsModule.Application.ReserveComponents.History.Dtos;
using ClaimsModule.Application.Results;
using ClaimsModule.Domain.ReserveComponents.History.Inputs;
using ClaimsModule.Domain.Roles;
using FluentValidation;
using MediatR;

namespace ClaimsModule.Application.ReserveComponents.History.Commands;

public sealed record RejectReserveCommand(Guid ClaimId, Guid ReserveHistoryId, string Reason)
    : ICommand<Result<ReserveHistoryStatusDto>>
{
    public RejectHistoryInput ToInput(UserRole role, Guid actorId, DateTimeOffset now)
    {
        return new RejectHistoryInput(ReserveHistoryId, role, actorId, now)
        {
            Reason = Reason,
        };
    }
}

public sealed class RejectReserveCommandValidator : AbstractValidator<RejectReserveCommand>
{
    public RejectReserveCommandValidator()
    {
        RuleFor(command => command.ClaimId).NotEmpty();
        RuleFor(command => command.ReserveHistoryId).NotEmpty();
        RuleFor(command => command.Reason).NotNull().MaximumLength(ReserveFieldLengths.ReviewReason);
    }
}

public sealed class RejectReserveCommandHandler(
    IReserveComponentRepository components,
    IUnitOfWork unitOfWork,
    IClock clock,
    ICurrentUser user) : IRequestHandler<RejectReserveCommand, Result<ReserveHistoryStatusDto>>
{
    public async Task<Result<ReserveHistoryStatusDto>> Handle(
        RejectReserveCommand command,
        CancellationToken cancellationToken)
    {
        var loaded = await components.GetByHistoryAsync(command.ClaimId, command.ReserveHistoryId, cancellationToken);

        if (!loaded.IsSuccess)
        {
            return loaded.ToFailure<ReserveHistoryStatusDto>();
        }

        var input = command.ToInput(user.Role, user.Id, clock.UtcNow);
        var result = loaded.Value.RejectHistory(input).ToResult();

        if (result.IsSuccess)
        {
            await unitOfWork.CommitAsync(cancellationToken);
        }

        return result.Map(move => new ReserveHistoryStatusDto(command.ReserveHistoryId, move.From.Name, move.To.Name));
    }
}
