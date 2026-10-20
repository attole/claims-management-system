using ClaimsModule.Application.Abstractions;
using ClaimsModule.Application.Claims.Contracts;
using ClaimsModule.Application.Messaging;
using ClaimsModule.Application.ReserveComponents.Contracts;
using ClaimsModule.Application.ReserveComponents.History.Dtos;
using ClaimsModule.Application.Results;
using ClaimsModule.Domain.ReserveComponents.History.Inputs;
using ClaimsModule.Domain.Roles;
using FluentValidation;
using MediatR;

namespace ClaimsModule.Application.ReserveComponents.History.Commands;

public sealed record ApproveReserveCommand(Guid ClaimId, Guid ReserveHistoryId)
    : ICommand<Result<ReserveHistoryStatusDto>>
{
    public string Reason { get; init; } = string.Empty;

    public ApproveHistoryInput ToInput(
        decimal approvedTotal,
        bool hasLimitOverride,
        UserRole role,
        Guid actorId,
        DateTimeOffset now)
    {
        return new ApproveHistoryInput(ReserveHistoryId, approvedTotal, hasLimitOverride, role, actorId, now)
        {
            Reason = Reason ?? string.Empty,
        };
    }
}

public sealed class ApproveReserveCommandValidator : AbstractValidator<ApproveReserveCommand>
{
    public ApproveReserveCommandValidator()
    {
        RuleFor(command => command.ClaimId).NotEmpty();
        RuleFor(command => command.ReserveHistoryId).NotEmpty();
        RuleFor(command => command.Reason).MaximumLength(ReserveFieldLengths.ReviewReason);
    }
}

public sealed class ApproveReserveCommandHandler(
    IClaimRepository claims,
    IReserveComponentRepository components,
    IClaimReserveReader reserves,
    IUnitOfWork unitOfWork,
    IClock clock,
    ICurrentUser user) : IRequestHandler<ApproveReserveCommand, Result<ReserveHistoryStatusDto>>
{
    public async Task<Result<ReserveHistoryStatusDto>> Handle(
        ApproveReserveCommand command,
        CancellationToken cancellationToken)
    {
        var loadedClaim = await claims.GetAsync(command.ClaimId, cancellationToken);

        if (!loadedClaim.IsSuccess)
        {
            return loadedClaim.ToFailure<ReserveHistoryStatusDto>();
        }

        var loaded = await components.GetByHistoryAsync(command.ClaimId, command.ReserveHistoryId, cancellationToken);

        if (!loaded.IsSuccess)
        {
            return loaded.ToFailure<ReserveHistoryStatusDto>();
        }

        var facts = await reserves.GetFactsAsync(command.ClaimId, cancellationToken);
        var input = command.ToInput(
            facts.ApprovedTotal,
            loadedClaim.Value.HasAggregateLimitOverride,
            user.Role,
            user.Id,
            clock.UtcNow);
        var result = loaded.Value.ApproveHistory(input).ToResult();

        if (result.IsSuccess)
        {
            await unitOfWork.CommitAsync(cancellationToken);
        }

        return result.Map(move => new ReserveHistoryStatusDto(command.ReserveHistoryId, move.From.Name, move.To.Name));
    }
}
